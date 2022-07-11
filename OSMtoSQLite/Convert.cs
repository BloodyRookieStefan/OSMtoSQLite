using OSMConverter.lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter
{
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
            OSMtoSQLite(file, sql, new Filter[] {Filter.None});
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
            OSMtoSQLite(file, sql, new Filter[] { filter });
        }

        /// <summary>
        /// Create new SQLite database out of OSM file
        /// </summary>
        /// <param name="file">Input OSM file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">Multiple OSM Filters</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void OSMtoSQLite(string file, string sql, Filter[] filter)
        {
            // Input checks
            if (!Path.GetExtension(file).ToLower().Equals(".osm"))
                throw new InvalidOperationException("Wrong file extension");
            if (filter.Length == 1 && filter[0] == Filter.None)
                InputChecks(file, sql, false);
            else
                InputChecks(file, sql, true);

            // Decompress
            // -- DEBUG
            ComboundBoxPoint top = new ComboundBoxPoint();
            top.Point = "49.175055";
            top.Shift = "9.190164";
            ComboundBoxPoint bottom = new ComboundBoxPoint();
            bottom.Point = "49.112747";
            bottom.Shift = "9.270416";
            // -----
            string[] files = Decompress.Extract(file, top, bottom, filter);

            // Read file
            //OSMReader.Read(file);
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
            BZ2toSQLite(file, sql, new Filter[] { Filter.None });
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
            BZ2toSQLite(file, sql, new Filter[] {filter});
        }

        /// <summary>
        /// Create new SQLite database out of BZ2 file
        /// </summary>
        /// <param name="file">Input BZ2 file</param>
        /// <param name="sql">Path to target SQLite database</param>
        /// <param name="filter">Multiple OSM Filters</param>
        /// <exception cref="InvalidOperationException">Wrong file extension</exception>
        public static void BZ2toSQLite(string file, string sql, Filter[] filter)
        {
            // Input checks
            if (!Path.GetExtension(file).ToLower().Equals(".bz2"))
                throw new InvalidOperationException("Wrong file extension");
            if (filter.Length == 1 && filter[0] == Filter.None)
                InputChecks(file, sql, false);
            else
                InputChecks(file, sql, true);

            // Decompress
            // -- DEBUG
            ComboundBoxPoint top = new ComboundBoxPoint();
            top.Point = "49.175055";
            top.Shift = "9.190164";
            ComboundBoxPoint bottom = new ComboundBoxPoint();
            bottom.Point = "49.112747";
            bottom.Shift = "9.270416";
            // -----
            string[] files = Decompress.Extract(file, top, bottom, filter);

            // Read file
            //OSMReader.Read(file);
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
        /// <exception cref="InvalidOperationException">SQLite database already exists</exception>
        private static void InputChecks(string file, string sql, bool javaNeeded)
        {
            if(javaNeeded && !Dependencies.CheckJavaInstallation())
                throw new Exception("No valid Java version found");
            if (!File.Exists(file))
                throw new FileNotFoundException(file);
            if (File.Exists(sql))
                throw new InvalidOperationException("SQLite database already exist");
        }
    }
}
