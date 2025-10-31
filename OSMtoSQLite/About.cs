using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

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
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
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

        /// <summary>
        /// Gets the company name from the assembly metadata.
        /// </summary>
        /// <returns>The company name.</returns>
        public static string GetCompany()
        {
            var attr = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCompanyAttribute>();
            return attr?.Company ?? string.Empty;
        }

        /// <summary>
        /// Gets the product name from the assembly metadata.
        /// </summary>
        /// <returns>The product name.</returns>
        public static string GetProduct()
        {
            var attr = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyProductAttribute>();
            return attr?.Product ?? string.Empty;
        }

        /// <summary>
        /// Gets the copyright information from the assembly metadata.
        /// </summary>
        /// <returns>The copyright string.</returns>
        public static string GetCopyright()
        {
            var attr = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyCopyrightAttribute>();
            return attr?.Copyright ?? string.Empty;
        }

        /// <summary>
        /// Gets the assembly's informational version (if set).
        /// </summary>
        /// <returns>The informational version string.</returns>
        public static string GetInformationalVersion()
        {
            var attr = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attr?.InformationalVersion ?? string.Empty;
        }
    }
}
