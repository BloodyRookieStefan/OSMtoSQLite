/* 
Plan (pseudocode, detailed):
1. Add a top-level comment block describing the intent: add XML documentation for all types and members in this file.
2. For the `ComboundBoxPoint` struct:
   - Add a `<summary>` describing the pair of coordinate strings (Point/Shift).
   - Document each property with `<summary>`.
3. For the `DataFilter` class:
   - Add a `<summary>` describing responsibility: orchestrates optional bounding-box extraction and parallel filtering using Osmosis.
   - Document static fields (MaxThreads, CurrentThreads, Source, NorthWest, SouthEast, Filters, Sources) with concise `<summary>` tags.
   - Document `PreProcess` method with `<summary>`, `<param>` for each parameter, `<returns>`, and remarks about side effects (temporary files).
   - Document `BoundingBox` and `Filtering` private methods with `<summary>` and any thrown exceptions or behaviors.
   - Document `HandleThreadDone` with `<summary>` and parameter descriptions.
4. For the nested `Worker` class:
   - Add a `<summary>` describing its purpose: running osmosis commands for filters or bounding-box operations.
   - Document the `ThreadDone` event.
   - Document private fields with `<summary>`.
   - Document constructors with `<summary>` and `<param>` entries.
   - Document `RunFilter` with `<summary>`, `<exception>` for invalid filters, and note that it raises the `ThreadDone` event.
   - Document `RunBoundingBox` with `<summary>`.
   - Document `StartOsmosis` with `<summary>`, `<param>`, and `<exception>` if Osmosis not found.
   - Document `GetPathOSMOSIS` with `<summary>` and `<returns>`.
5. Preserve all existing logic and signatures; only add XML doc comments.
6. Place the detailed pseudocode above as a block comment (this block) in the file so the plan appears before the annotated code.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSMConverter.lib
{
    /// <summary>
    /// Represents a point used for a bounding box cut where the coordinate components
    /// are stored as string pairs (Point and Shift).
    /// </summary>
    internal struct ComboundBoxPoint
    {
        /// <summary>
        /// Gets or sets the primary coordinate value (e.g., latitude).
        /// Stored as string to match external command formatting.
        /// </summary>
        public string Point { get; set; }

        /// <summary>
        /// Gets or sets the secondary coordinate value (e.g., longitude or shift).
        /// Stored as string to match external command formatting.
        /// </summary>
        public string Shift { get; set; }
    }

    /// <summary>
    /// Responsible for preparing OSM input data and applying optional filtering and bounding-box
    /// operations. This class orchestrates temporary file creation and executes parallel filter
    /// operations using <see cref="Worker"/> instances.
    /// </summary>
    internal class DataFilter
    {
        /// <summary>
        /// Maximum number of parallel threads used for filtering.
        /// </summary>
        private readonly static int MaxThreads = 4;                                                     // Max parallel thread for filtering

        /// <summary>
        /// Input source file path (OSM data). May be replaced by a bounding-box trimmed file.
        /// </summary>
        private static string Source;                                                                   // Input source file (OSM data)

        /// <summary>
        /// Bounding box top-left point (north-west) when bounding-box trimming is requested.
        /// </summary>
        private static ComboundBoxPoint? NorthWest;                                                     // Combound box point TOP

        /// <summary>
        /// Bounding box bottom-right point (south-east) when bounding-box trimming is requested.
        /// </summary>
        private static ComboundBoxPoint? SouthEast;                                                     // Combound box point BOTTOM

        /// <summary>
        /// Selected filters to apply to the source data. The collection is consumed by the filter loop.
        /// </summary>
        private static List<Filter> Filters;                                                            // Selected filters

        /// <summary>
        /// Output array that stores paths to filtered output files. Index 0 is the source (possibly new)
        /// and subsequent indices correspond to specific filter outputs.
        /// </summary>
        private static string[] Sources;                                                                // Output array for path to filtered files

        /// <summary>
        /// Prepares the input data by optionally performing a bounding-box cut and then running filters
        /// in parallel. Temporary files are created in the process.
        /// </summary>
        /// <param name="source">Path to the input OSM file to process.</param>
        /// <param name="northWest">Optional north-west bounding box point. If null, no top-left coordinate is used.</param>
        /// <param name="southEast">Optional south-east bounding box point. If null, no bottom-right coordinate is used.</param>
        /// <param name="filters">List of filters to apply. The method removes the <see cref="Filter.None"/> value if present.</param>
        /// <returns>
        /// An array of file paths to produced files. The index mapping corresponds to how filters are stored in this class:
        /// index 0 contains the (potentially trimmed) source, other indices contain paths to specific filter outputs.
        /// </returns>
        internal static string[] PreProcess(string source, ComboundBoxPoint? northWest, ComboundBoxPoint? southEast, List<Filter> filters)
        {
            // Get in clear state
            Sources = new string[Enum.GetNames(typeof(Filter)).Length];

            // Remove "None" filter
            filters.Remove(Filter.None);

            // Do a cleanup before we start
            TempFolder.CleanUpTempFolder();

            // Set vars
            Source = source;
            NorthWest = northWest;
            SouthEast = southEast;
            Filters = new List<Filter>(filters);

            // Cut first bounding box
            BoundingBox();
            // Filter data
            Filtering();

            return Sources;
        }

        /// <summary>
        /// Performs an optional bounding-box cut on the input OSM file. If both <see cref="NorthWest"/>
        /// and <see cref="SouthEast"/> are provided, a temporary bounding-box trimmed file is created and used
        /// as the new source for subsequent filtering operations.
        /// </summary>
        private static void BoundingBox()
        {
            Timers.StartTimer(LibTimers.Filter_BoundingBox);

            Sources[0] = Source;

            // If bounding box is given cut first
            if (NorthWest != null && SouthEast != null)
            {
                string newSource = Path.Combine(TempFolder.GetTempFolder(), "BoundingBox.osm");
                Worker w = new Worker(Sources[0], newSource, NorthWest, SouthEast);
                w.RunBoundingBox();
                Sources[0] = newSource;      // Take bounding box as new source file
            }

            Timers.StopTimer();
        }

        /// <summary>
        /// Runs the configured filters in parallel up to <see cref="MaxThreads"/> concurrent workers.
        /// Jeder Filter wird als Task ausgeführt, die Parallelität wird mit SemaphoreSlim gesteuert.
        /// Die Methode blockiert, bis alle Filter abgeschlossen sind.
        /// </summary>
        /// <exception cref="Exception">Thrown when an invalid filter is encountered while creating workers.</exception>
        private static void Filtering()
        {
            Timers.StartTimer(LibTimers.Filter_Filtering);

            var semaphore = new SemaphoreSlim(MaxThreads);
            var tasks = new List<Task>();
            var filterList = new List<Filter>(Filters); // Kopie, da wir parallel arbeiten

            foreach (var f in filterList)
            {
                string outPath = null;
                int i;
                switch (f)
                {
                    case Filter.Buildings:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Highways.osm");
                        i = 1;
                        break;
                    case Filter.Highway:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Buildings.osm");
                        i = 2;
                        break;
                    case Filter.Wood:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Wood.osm");
                        i = 3;
                        break;
                    case Filter.Waterway:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Waterway.osm");
                        i = 4;
                        break;
                    case Filter.Railway:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Railway.osm");
                        i = 5;
                        break;
                    default:
                        throw new Exception("Invalid filter");
                }
                Sources[i] = outPath;

                var worker = new Worker(Sources[0], outPath, f);
                var task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        worker.RunFilter();
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            Timers.StopTimer();
        }
    }

    /// <summary>
    /// Executes individual osmosis command lines for either bounding-box extraction or filter-based extraction.
    /// Each instance encapsulates a single task and can be run on a background thread.
    /// </summary>
    internal class Worker
    {
        /// <summary>
        /// Event triggered when the worker has completed its task.
        /// Subscribers can use this to track concurrent worker completion.
        /// </summary>
        public event EventHandler ThreadDone;

        /// <summary>
        /// Source file path used as input for the osmosis command.
        /// </summary>
        private string SourceFile = null;                   // Source file as input

        /// <summary>
        /// Output file path written by the osmosis command.
        /// </summary>
        private string OutputFile = null;                   // Source file as output

        /// <summary>
        /// The selected OpenStreetMap filter for this worker. If <see cref="Filter.None"/>, the worker
        /// behaves according to bounding-box settings or throws for invalid operations.
        /// </summary>
        private Filter OSMFilter = Filter.None;             // Open street map filter

        /// <summary>
        /// Bounding box top coordinate when performing bounding-box operations.
        /// </summary>
        private ComboundBoxPoint? Top = null;               // Bounding box TOP

        /// <summary>
        /// Bounding box bottom coordinate when performing bounding-box operations.
        /// </summary>
        private ComboundBoxPoint? Bottom = null;            // Bounding box BOTTOM

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class configured to run a specific filter.
        /// </summary>
        /// <param name="source">Path to the input OSM file.</param>
        /// <param name="output">Path to the output OSM file to create.</param>
        /// <param name="filter">The <see cref="Filter"/> to extract from the source.</param>
        public Worker(string source, string output, Filter filter)
        {
            SourceFile = source;
            OutputFile = output;

            OSMFilter = filter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class configured to perform a bounding-box cut.
        /// </summary>
        /// <param name="source">Path to the input OSM file.</param>
        /// <param name="output">Path to the output OSM file to create.</param>
        /// <param name="top">Top (north-west) point of the bounding box.</param>
        /// <param name="bottom">Bottom (south-east) point of the bounding box.</param>
        public Worker(string source, string output, ComboundBoxPoint? top, ComboundBoxPoint? bottom)
        {
            SourceFile = source;
            OutputFile = output;

            Top = top;
            Bottom = bottom;
        }

        /// <summary>
        /// Runs the configured filter by invoking Osmosis with the appropriate command-line arguments.
        /// This method is intended to be run on a background thread.
        /// </summary>
        /// <remarks>
        /// The method calls <see cref="StartOsmosis(string)"/> with a pre-built osmosis command. After completion
        /// it raises the <see cref="ThreadDone"/> event to notify listeners that processing has finished.
        /// </remarks>
        /// <exception cref="Exception">Throws if an invalid filter is encountered.</exception>
        public void RunFilter()
        {
            switch (OSMFilter)
            {
                case Filter.Buildings:
                    StartOsmosis($"--read-xml file=\"{SourceFile}\" --tf accept-ways highway=* --used-node --write-xml file=\"{OutputFile}\"");
                    break;
                case Filter.Highway:
                    StartOsmosis($"--read-xml file=\"{SourceFile}\" --tf accept-ways building=* --used-node --write-xml file=\"{OutputFile}\"");
                    break;
                case Filter.Wood:
                    StartOsmosis($"--read-xml file=\"{SourceFile}\" --tf accept-ways wood=* --used-node --write-xml file=\"{OutputFile}\"");
                    break;
                case Filter.Waterway:
                    StartOsmosis($"--read-xml file=\"{SourceFile}\" --tf accept-ways waterway=* --used-node --write-xml file=\"{OutputFile}\"");
                    break;
                case Filter.Railway:
                    StartOsmosis($"--read-xml file=\"{SourceFile}\" --tf accept-ways railway=* --used-node --write-xml file=\"{OutputFile}\"");
                    break;
                default:
                    throw new Exception("Invalid filter");
            }

            // Fire thread done
            ThreadDone?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Runs a bounding-box extraction using Osmosis. The method builds the appropriate osmosis
        /// command using <see cref="Top"/> and <see cref="Bottom"/> values and executes it synchronously.
        /// </summary>
        public void RunBoundingBox()
        {
            StartOsmosis($"--read-xml file=\"{SourceFile}\" --bounding-box top={Top.Value.Point} left={Top.Value.Shift} bottom={Bottom.Value.Point} right={Bottom.Value.Shift} --write-xml file=\"{OutputFile}\"");
        }

        /// <summary>
        /// Starts an external Osmosis process with the provided command fragment.
        /// The method locates the osmosis executable and invokes it via the system command shell.
        /// </summary>
        /// <param name="command">Osmosis command arguments to append to the osmosis executable call.</param>
        /// <exception cref="Exception">Thrown when the Osmosis executable cannot be found at the expected location.</exception>
        private void StartOsmosis(string command)
        {
            // Get Osmosis path
            string osmosis = GetPathOSMOSIS();
            if (osmosis == null)
                throw new Exception("Osmosis not found");

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.WorkingDirectory = Path.GetDirectoryName(osmosis);
            startInfo.Arguments = $"/C osmosis {command}";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        /// <summary>
        /// Attempts to resolve the path to the Osmosis executable relative to the application's dependencies folder.
        /// </summary>
        /// <returns>
        /// The full path to the osmosis executable if found; otherwise <c>null</c>.
        /// </returns>
        private string GetPathOSMOSIS()
        {
            string location = Path.Combine(Dependencies.AssemblyDirectory, @"Osmosis\bin\osmosis");

            if (File.Exists(location))
                return location;

            return null;
        }
    }
}
