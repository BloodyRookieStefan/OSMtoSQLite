using OSMConverter.lib;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OSMConverter
{
    /*
    Plan (pseudocode, detailed):
    1. Add XML documentation to the `Filter` enum describing its purpose and each member.
    2. Add XML documentation to the `Convert` class explaining its responsibility (converting OSM/BZ2 files to SQLite).
    3. For each public static overload method (OSMtoSQLite / BZ2toSQLite):
       - Add `<summary>` explaining what the overload does.
       - Add `<param>` entries for each parameter.
       - Add `<exception>` tags matching existing thrown exceptions where applicable.
       - Add `<remarks>` when behavior is delegated to other overloads.
    4. For the central `BZ2toSQLite` overload that does the actual work:
       - Document the filtering and bounding-box behavior.
       - Describe side effects (creates databases, reads OSM, writes to SQL).
       - Document possible exceptions and input validation behavior.
    5. For the private `InputChecks` method:
       - Document each validation step (file extension, Java dependency, file existence).
       - Document thrown exceptions precisely.
    6. Preserve existing code logic and signatures exactly; only add XML doc comments.
    7. Keep comments concise but clear so IDE tooltips are useful.
    8. Output the full file content replacing previous file contents.
    */

    /// <summary>
    /// Filters that can be applied when converting OSM/BZ2 data to SQLite databases.
    /// </summary>
    public enum Filter
    {
        /// <summary>
        /// No filtering; include all data.
        /// </summary>
        None,
        /// <summary>
        /// Include building-related data.
        /// </summary>
        Buildings,
        /// <summary>
        /// Include highway (roads, paths, etc.) data.
        /// </summary>
        Highway,
        /// <summary>
        /// Include wood/forest-related data.
        /// </summary>
        Wood,
        /// <summary>
        /// Include waterway-related data (rivers, streams, etc.).
        /// </summary>
        Waterway,
        /// <summary>
        /// Include railway-related data.
        /// </summary>
        Railway,
    }

    /// <summary>
    /// Provides methods to convert OpenStreetMap (OSM) or BZ2-compressed OSM files into SQLite databases.
    /// </summary>
    /// <remarks>
    /// This class exposes several overloads that accept different combinations of input file types,
    /// output paths, filters and optional bounding box coordinates. The main work happens in the
    /// `BZ2toSQLite` method which performs preprocessing, decompression (via `DataFilter`),
    /// OSM parsing (via `OSMReader`) and persistence (via `SQLController`).
    /// </remarks>
    public class Convert
    {
        #region OSM to SQLite
        /// <summary>
        /// Create a new SQLite database from an input OSM file.
        /// </summary>
        /// <param name="file">Path to the input OSM file (can be .osm or .bz2).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void OSMtoSQLite(string file, string sql)
        {
            BZ2toSQLite(file, sql, new List<Filter> { Filter.None });
        }

        /// <summary>
        /// Create a new SQLite database from an input OSM file and apply the specified filter.
        /// </summary>
        /// <param name="file">Path to the input OSM file (can be .osm or .bz2).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <param name="filter">Single <see cref="Filter"/> value to apply during conversion.</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void OSMtoSQLite(string file, string sql, Filter filter)
        {
            BZ2toSQLite(file, sql, new List<Filter> { filter });
        }

        /// <summary>
        /// Create a new SQLite database from an input OSM file applying multiple filters.
        /// </summary>
        /// <param name="file">Path to the input OSM file (can be .osm or .bz2).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <param name="filter">List of <see cref="Filter"/> values to apply during conversion.</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void OSMtoSQLite(string file, string sql, List<Filter> filter)
        {
            BZ2toSQLite(file, sql, filter, null, null);
        }

        /// <summary>
        /// Create a new SQLite database from an input OSM file applying multiple filters and restricting input by a bounding box.
        /// </summary>
        /// <param name="file">Path to the input OSM file (can be .osm or .bz2).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <param name="filter">List of <see cref="Filter"/> values to apply during conversion.</param>
        /// <param name="northWest">North-west corner of the bounding box to restrict processing (nullable).</param>
        /// <param name="southEast">South-east corner of the bounding box to restrict processing (nullable).</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void OSMtoSQLite(string file, string sql, List<Filter> filter, Point? northWest, Point? southEast)
        {
            BZ2toSQLite(file, sql, filter, northWest, southEast);
        }
        #endregion

        #region BZ2 to SQLLite
        /// <summary>
        /// Create a new SQLite database from a BZ2-compressed OSM file.
        /// </summary>
        /// <param name="file">Path to the input BZ2 file (or .osm file).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void BZ2toSQLite(string file, string sql)
        {
            BZ2toSQLite(file, sql, new List<Filter> { Filter.None });
        }

        /// <summary>
        /// Create a new SQLite database from a BZ2-compressed OSM file applying a single filter.
        /// </summary>
        /// <param name="file">Path to the input BZ2 file (or .osm file).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <param name="filter">Single <see cref="Filter"/> value to apply during conversion.</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void BZ2toSQLite(string file, string sql, Filter filter)
        {
            BZ2toSQLite(file, sql, new List<Filter> { filter });
        }

        /// <summary>
        /// Create a new SQLite database from a BZ2-compressed OSM file applying multiple filters.
        /// </summary>
        /// <param name="file">Path to the input BZ2 file (or .osm file).</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written.</param>
        /// <param name="filter">List of <see cref="Filter"/> values to apply during conversion.</param>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        public static void BZ2toSQLite(string file, string sql, List<Filter> filter)
        {
            BZ2toSQLite(file, sql, filter, null, null);
        }

        /// <summary>
        /// Create new SQLite database(s) from a BZ2-compressed OSM file with optional filtering and bounding-box extraction.
        /// </summary>
        /// <param name="file">Path to the input BZ2 or OSM file.</param>
        /// <param name="sql">Directory path where generated SQLite database(s) will be written. Database files are created inside this directory.</param>
        /// <param name="filter">List of <see cref="Filter"/> values to apply. Use <see cref="Filter.None"/> to produce an unfiltered output.</param>
        /// <param name="northWest">Optional north-west corner of a bounding box (if provided, processing is restricted to this box).</param>
        /// <param name="southEast">Optional south-east corner of a bounding box (if provided, processing is restricted to this box).</param>
        /// <remarks>
        /// This method:
        /// - Validates input via <see cref="InputChecks(string,string,bool)"/>.
        /// - Calls <see cref="DataFilter.PreProcess(string, ComboundBoxPoint, ComboundBoxPoint, List{Filter})"/> to decompress and optionally filter/extract by bounding box.
        /// - For each resulting file, creates a SQLite database using <see cref="SQLController.CreateDatabase(string)"/>,
        ///   parses OSM content via <see cref="OSMReader.Read(string)"/> and writes parsed data to SQL with <see cref="SQLController.SendDataToSQL(System.Collections.Generic.Dictionary<long, OSMConverter.lib.Node>, System.Collections.Generic.Dictionary<long, OSMConverter.lib.Way>, System.Collections.Generic.Dictionary<long, OSMConverter.lib.Relation>)"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the input file extension is invalid.</exception>
        /// <exception cref="Exception">Thrown when a required Java installation is not found (if required for preprocessing).</exception>
        /// <exception cref="FileNotFoundException">Thrown when the input file does not exist.</exception>
        public static void BZ2toSQLite(string file, string sql, List<Filter> filter, Point? northWest, Point? southEast)
        {
            // Input checks
            if (filter.Count == 1 && filter[0] == Filter.None && northWest == null && southEast == null)
                InputChecks(file, sql, false);
            else
                InputChecks(file, sql, true);

            string[] files;
            if (northWest != null && southEast != null)
            {
                ComboundBoxPoint c_northWest = new ComboundBoxPoint();
                ComboundBoxPoint c_southEast = new ComboundBoxPoint();

                c_northWest.Point = northWest.Value.X.ToString().Replace(',', '.');
                c_northWest.Shift = northWest.Value.Y.ToString().Replace(',', '.');
                c_southEast.Point = southEast.Value.X.ToString().Replace(',', '.');
                c_southEast.Shift = southEast.Value.Y.ToString().Replace(',', '.');

                // Decompress
                files = DataFilter.PreProcess(file, c_northWest, c_southEast, filter.Distinct().ToList());
            }
            else
            {
                // Decompress
                files = DataFilter.PreProcess(file, null, null, filter.Distinct().ToList());
            }

            Timers.StartTimer(LibTimers.SQL_Storing);
            for (int i = 0; i < files.Length; i++)
            {
                // Skip null files
                if (files[i] == null)
                    continue;

                // Select databse name
                string dbName = null;
                if (i == 0)
                    dbName = "BoundingBox";
                else
                    dbName = ((Filter)i).ToString();

                // Create new empty databse
                SQLController.CreateDatabase(Path.Combine(sql, $"{dbName}.sqlite"));
                // Read file
                OSMReader.Read(files[i]);
                // Send data to SQL databse
                SQLController.SendDataToSQL(OSMReader.Nodes, OSMReader.Ways, OSMReader.Relations);
            }
            Timers.StopTimer();
        }
        #endregion

        /// <summary>
        /// Validates input parameters and required dependencies before conversion.
        /// </summary>
        /// <param name="file">Path to the input file to validate (expected extension: <c>.bz2</c> or <c>.osm</c>).</param>
        /// <param name="sql">Target directory where SQLite database(s) will be written.</param>
        /// <param name="javaNeeded">
        /// If <c>true</c>, the method checks for a valid Java installation by calling <see cref="Dependencies.CheckJavaInstallation"/>.
        /// If Java is required but not found, an exception is thrown.
        /// </param>
        /// <exception cref="Exception">Thrown when Java is required but no valid Java installation is found.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the input file does not exist.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the input file has an unsupported extension.</exception>
        private static void InputChecks(string file, string sql, bool javaNeeded)
        {
            if(!Path.GetExtension(file).ToLower().Equals(".bz2") && !Path.GetExtension(file).ToLower().Equals(".osm"))
                throw new InvalidOperationException("Wrong input file extension");

            if (javaNeeded && !Dependencies.CheckJavaInstallation())
                throw new Exception("No valid Java version found");
            if (!File.Exists(file))
                throw new FileNotFoundException(file);
        }
    }
}
