using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter
{
    /// <summary>
    /// Provides application metadata such as name, version, description, and source URL.
    /// </summary>
    public class About
    {
        /// <summary>
        /// Gets the application name.
        /// </summary>
        /// <returns>The name of the application.</returns>
        public static string GetAppName()
        {
            return "OSM to SQLite Converter";
        }   

        /// <summary>
        /// Gets the application version.
        /// </summary>
        /// <returns>The version of the application.</returns>
        public static string GetAppVersion()
        {
            return "1.0.0";
        }

        /// <summary>
        /// Gets the application description.
        /// </summary>
        /// <returns>A description of the application.</returns>
        public static string GetAppDescription()
        {
            return "A tool to convert OpenStreetMap data into SQLite database format.";
        }

        /// <summary>
        /// Gets the source URL of the application.
        /// </summary>
        /// <returns>The URL to the application's source code repository.</returns>
        public static string GetSoruce()
        {
            return "https://github.com/BloodyRookieStefan/OSMtoSQLite/tree/main";
        }
    }
}
