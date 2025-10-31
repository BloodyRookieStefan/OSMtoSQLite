using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter.lib
{
    /// <summary>
    /// Provides helper methods for creating, retrieving and cleaning a program-specific temporary folder.
    /// </summary>
    /// <remarks>
    /// The temp folder path is created by combining the system temporary path returned by <see cref="Path.GetTempPath"/>
    /// with the folder name "OSMConverter". Methods in this class operate on that folder.
    /// </remarks>
    internal class TempFolder
    {
        /// <summary>
        /// Gets the program temp folder path and creates the folder if it does not exist.
        /// </summary>
        /// <returns>
        /// The full path to the program-specific temporary folder (e.g. Path.GetTempPath() + "OSMConverter").
        /// </returns>
        internal static string GetTempFolder()
        {
            return GetTempFolder(true);
        }

        /// <summary>
        /// Gets the program temp folder path.
        /// </summary>
        /// <param name="createFolder">
        /// If true, the method ensures the temp folder exists by creating it when it does not exist.
        /// If false, the method returns the path without creating the folder.
        /// </param>
        /// <returns>
        /// The full path to the program-specific temporary folder (e.g. Path.GetTempPath() + "OSMConverter").
        /// </returns>
        /// <remarks>
        /// This method combines <see cref="Path.GetTempPath"/> with the fixed folder name "OSMConverter".
        /// </remarks>
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
        /// Cleans up the program temp folder by removing all files and deleting the folder.
        /// </summary>
        /// <remarks>
        /// - If the temp folder does not exist, this method returns without action.
        /// - The method deletes all files returned by <see cref="Directory.GetFiles(string)"/> within the temp folder,
        ///   then deletes the folder and its contents via <see cref="Directory.Delete(string,bool)"/>.
        /// </remarks>
        /// <exception cref="IOException">
        /// Thrown when an I/O error occurs while deleting files or the directory (for example, files in use).
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the caller does not have the required permission to delete files or the directory.
        /// </exception>
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
            Directory.Delete(GetTempFolder(false), true);    
        }
    }
}
