using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter
{
    /// <summary>
    /// Provides helper methods for detecting external dependencies and locating
    /// the executing assembly directory.
    /// </summary>
    internal class Dependencies
    {
        /// <summary>
        /// Minimum required Java major version component.
        /// </summary>
        /// <remarks>
        /// The value is used together with <see cref="Java_Minor"/> to represent the minimum
        /// required Java version. Default values indicate Java 1.8 is required.
        /// </remarks>
        private static int Java_Major = 1;

        /// <summary>
        /// Minimum required Java minor version component.
        /// </summary>
        /// <remarks>
        /// The value is used together with <see cref="Java_Major"/> to represent the minimum
        /// required Java version. Default values indicate Java 1.8 is required.
        /// </remarks>
        private static int Java_Minor = 8;

        /// <summary>
        /// Check if Java is installed on the system and meets the minimum required version.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a Java executable is found and its parsed major/minor version
        /// meets or exceeds the configured minimum; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Implementation details:
        /// - Starts the external process "java.exe" with the "-version" argument.
        /// - The JRE/JDK typically writes version information to the standard error stream,
        ///   so this method reads <see cref="Process.StandardError"/>.
        /// - The code parses the first error line, extracts the quoted version token,
        ///   and splits it on '.' to obtain major/minor components.
        /// - Any exceptions arising from process start, IO or parsing are swallowed and the
        ///   method returns <c>false</c>. The method intentionally does not throw.
        /// - Note: This method performs a simple textual parse and may not handle all
        ///   Java version string formats; the behavior is preserved from the original implementation.
        /// </remarks>
        internal static bool CheckJavaInstallation()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = "-version";
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;

                Process pr = Process.Start(psi);
                string strOutput = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");
                int javaMajor = System.Convert.ToInt32(strOutput.Split('.')[0]);
                int javaMinor = System.Convert.ToInt32(strOutput.Split('.')[1]);
#if DEBUG
                Console.WriteLine($"DEBUG: Java version detected: {javaMajor}.{javaMinor}");
#endif
                // At least Java 1.8 needed
                if (javaMajor == Java_Major && javaMinor == Java_Minor)
                    return true;
                else if(javaMinor > Java_Major)
                    return true;
            }
            catch
            { }

            return false;
        }

        /// <summary>
        /// Gets the directory path of the executing assembly.
        /// </summary>
        /// <value>
        /// A string containing the full directory path where the currently executing
        /// assembly is located. Returns <c>null</c> if the path cannot be determined.
        /// </value>
        /// <remarks>
        /// This property uses <see cref="Assembly.GetExecutingAssembly().CodeBase"/> to
        /// obtain the assembly location as a URI, converts it to a local path using
        /// <see cref="Uri.UnescapeDataString"/>, and then returns the directory portion.
        /// </remarks>
        internal static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
