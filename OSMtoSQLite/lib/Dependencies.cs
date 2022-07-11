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
    internal class Dependencies
    {
        // Minimum Java version
        private static int Java_Major = 1;
        private static int Java_Minor = 8;

        /// <summary>
        /// Check if java is installed
        /// </summary>
        /// <returns>True if installed</returns>
        internal static bool CheckJavaInstallation()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "java.exe";
                psi.Arguments = " -version";
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;

                Process pr = Process.Start(psi);
                string strOutput = pr.StandardError.ReadLine().Split(' ')[2].Replace("\"", "");
#if DEBUG
                Console.WriteLine($"DEBUG: Java version detected: {strOutput}");
#endif
                return true;    
            }
            catch
            { }

            return false;
        }

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
