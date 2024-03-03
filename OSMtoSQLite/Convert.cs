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
    public enum Filter
    {
        None,
        Buildings,
        Highway,
        Wood,
        Waterway,
        Railway,
    }

    public class Convert
    {
        #region OSM to SQLite
        /// <summary>
        /// Create new SQLite database out of OSM file
        /// </summary>
        /// <param name="file">Input OSM file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void OSMtoSQLite(string file, string sql)
        {
            BZ2toSQLite(file, sql, new List<Filter> { Filter.None });
        }

        /// <summary>
        /// Create new SQLite database out of OSM file
        /// </summary>
        /// <param name="file">Input OSM file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">OSM Filter</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void OSMtoSQLite(string file, string sql, Filter filter)
        {
            BZ2toSQLite(file, sql, new List<Filter> { filter });
        }

        /// <summary>
        /// Create new SQLite database out of OSM file
        /// </summary>
        /// <param name="file">Input OSM file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">Multiple OSM Filters</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void OSMtoSQLite(string file, string sql, List<Filter> filter)
        {
            BZ2toSQLite(file, sql, filter, null, null);
        }

        /// <summary>
        /// Create new SQLite database out of OSM file
        /// </summary>
        /// <param name="file">Input BZ2 file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">Multiple OSM Filters</param>
        /// <param name="northWest">Point of bounding box - north west</param>
        /// <param name="southEast">Point of bounding box - south east</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void OSMtoSQLite(string file, string sql, List<Filter> filter, Point? northWest, Point? southEast)
        {
            BZ2toSQLite(file, sql, filter, northWest, southEast);
        }
        #endregion

        #region BZ2 to SQLLite
        /// <summary>
        /// Create new SQLite database out of BZ2 file
        /// </summary>
        /// <param name="file">Input BZ2 file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void BZ2toSQLite(string file, string sql)
        {
            BZ2toSQLite(file, sql, new List<Filter> { Filter.None });
        }

        /// <summary>
        /// Create new SQLite database out of BZ2 file
        /// </summary>
        /// <param name="file">Input BZ2 file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">OSM Filter</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void BZ2toSQLite(string file, string sql, Filter filter)
        {
            BZ2toSQLite(file, sql, new List<Filter> { filter });
        }

        /// <summary>
        /// Create new SQLite database out of BZ2 file
        /// </summary>
        /// <param name="file">Input BZ2 file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">Multiple OSM Filters</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void BZ2toSQLite(string file, string sql, List<Filter> filter)
        {
            BZ2toSQLite(file, sql, filter, null, null);
        }

        /// <summary>
        /// Create new SQLite database out of BZ2 file
        /// </summary>
        /// <param name="file">Input BZ2 file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">Multiple OSM Filters</param>
        /// <param name="northWest">Point of bounding box - north west</param>
        /// <param name="southEast">Point of bounding box - south east</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
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
        /// Input checks
        /// </summary>
        /// <param name="file">File to OSM data</param>
        /// <param name="sql">Target SQLite database</param>
        /// <param name="javaNeeded">Check for java installation</param>
        /// <exception cref="Exception">No Java installation found</exception>
        /// <exception cref="FileNotFoundException">Input OSM file does not exist</exception>
        /// <exception cref="InvalidOperationException">Wrong extension for input file</exception>
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
