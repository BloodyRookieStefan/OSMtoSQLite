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
    struct ComboundBoxPoint
    {
        public string Point { get; set; }
        public string Shift { get; set; }
    }

    internal class DataFilter
    {
        private readonly static int MaxThreads = 4;                                                     // Max parallel thread for filtering
        private static int CurrentThreads = 0;                                                          // Current running parallel threads

        private static string Source;                                                                   // Input source file (OSM data)
        private static ComboundBoxPoint? NorthWest;                                                     // Combound box point TOP
        private static ComboundBoxPoint? SouthEast;                                                     // Combound box point BOTTOM
        private static List<Filter> Filters;                                                            // Selected filters

        private static string[] Sources;                                                                // Output array for path to filtered files

        internal static TimeSpan ElapsedTime = TimeSpan.Zero;                                           // Elapst time for filter operation

        internal static string[] PreProcess(string source, ComboundBoxPoint? northWest, ComboundBoxPoint? southEast, List<Filter> filters)
        {
            // Start time
            DateTime start = DateTime.UtcNow;
            // Get in clear state
            Sources = new string[Enum.GetNames(typeof(Filter)).Length];
            ElapsedTime = TimeSpan.Zero;

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

            // End time
            ElapsedTime = DateTime.UtcNow - start;

            return Sources;
        }

        /// <summary>
        /// Cut OSM data if requested
        /// </summary>
        private static void BoundingBox()
        {
            Sources[0] = Source;

            // If bounding box is given cut first
            if (NorthWest != null && SouthEast != null)
            {
                string newSource = Path.Combine(TempFolder.GetTempFolder(), "BoundingBox.osm");
                Worker w = new Worker(Sources[0], newSource, NorthWest, SouthEast);
                w.RunBoundingBox();
                Sources[0] = newSource;      // Take bounding box as new source file
            }
        }

        /// <summary>
        /// Filter OSM data
        /// </summary>
        /// <exception cref="Exception">Invalid OSM filter</exception>
        private static void Filtering()
        {
            while (Filters.Count > 0 || CurrentThreads > 0)
            {
                if (Filters.Count > 0 && CurrentThreads < MaxThreads)
                {
                    // Start new thread
                    CurrentThreads++;

                    // Get next filter
                    Filter f = Filters[0];
                    Filters.Remove(f);

                    // Select output file name
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

                    // Create new worker and start thread
                    Worker w = new Worker(Sources[0], outPath, f);
                    w.ThreadDone += HandleThreadDone;

                    Thread t = new Thread(w.RunFilter);
                    t.Start();

                    // Save output file path
                    Sources[i] = outPath;
                }
                else
                {
                    // Wait till thread is free or all threads are finsihed
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        /// <summary>
        /// General thread done event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void HandleThreadDone(object sender, EventArgs e)
        {
            CurrentThreads--;
        }
    }

    internal class Worker
    {
        /// <summary>
        /// Event handler when thread is done
        /// </summary>
        public event EventHandler ThreadDone;

        private string SourceFile = null;                   // Source file as input
        private string OutputFile = null;                   // Source file as output
        private Filter OSMFilter = Filter.None;             // Open street map filter

        private ComboundBoxPoint? Top = null;               // Bounding box TOP
        private ComboundBoxPoint? Bottom = null;            // Bounding box BOTTOM

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source file path</param>
        /// <param name="output">Output file path</param>
        /// <param name="filter">Open streetmap filter</param>
        public Worker(string source, string output, Filter filter)
        {
            SourceFile = source;
            OutputFile = output;

            OSMFilter = filter;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source file path</param>
        /// <param name="output">Output file path</param>
        /// <param name="top">Bounding box TOP</param>
        /// <param name="bottom">Bounding box BOTTOM</param>
        public Worker(string source, string output, ComboundBoxPoint? top, ComboundBoxPoint? bottom)
        {
            SourceFile = source;
            OutputFile = output;

            Top = top;
            Bottom = bottom;
        }

        /// <summary>
        /// Run filter on OSM input file
        /// </summary>
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
            ThreadDone(this, EventArgs.Empty);
        }

        /// <summary>
        /// Run bounding box on OSM input file
        /// </summary>
        public void RunBoundingBox()
        {
            StartOsmosis($"--read-xml file=\"{SourceFile}\" --bounding-box top={Top.Value.Point} left={Top.Value.Shift} bottom={Bottom.Value.Point} right={Bottom.Value.Shift} --write-xml file=\"{OutputFile}\"");
        }

        /// <summary>
        /// Start Osmosis process
        /// </summary>
        /// <param name="command">Osmosis command</param>
        /// <exception cref="Exception">Osmosis could not be found</exception>
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
        /// Get path to OSMOSIS
        /// </summary>
        /// <returns>Return null if not located</returns>
        private string GetPathOSMOSIS()
        {
            string location = Path.Combine(Dependencies.AssemblyDirectory, @"Osmosis\bin\osmosis");

            if (File.Exists(location))
                return location;

            return null;
        }
    }
}
