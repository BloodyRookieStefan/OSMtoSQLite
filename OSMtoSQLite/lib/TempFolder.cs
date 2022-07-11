using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter.lib
{
    internal class TempFolder
    {
        /// <summary>
        /// Get program temp folder
        /// </summary>
        /// <returns>Path to temp folder</returns>
        internal static string GetTempFolder()
        {
            return GetTempFolder(true);
        }

        /// <summary>
        /// Get program temp folder
        /// </summary>
        /// <param name="createFolder">Create temp folder if not exists</param>
        /// <returns>Path to temp folder</returns>
        internal static string GetTempFolder(bool createFolder)
        {
            // Temp folder
            string tempFolder = Path.Combine(Path.GetTempPath(), "OSMConverter");
            // Create folder if not exists
            if (createFolder && !Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            return tempFolder;
        }

        /// <summary>
        /// Cleanup temp folder
        /// </summary>
        internal static void CleanUpTempFolder()
        {
            if (!Directory.Exists(GetTempFolder(false)))
                return;

            // Remove all files
            foreach(var file in Directory.GetFiles(GetTempFolder()))
            {
                File.Delete(file);  
            }

            // Remove folder
            Directory.Delete(GetTempFolder(), true);    
        }
    }
}
