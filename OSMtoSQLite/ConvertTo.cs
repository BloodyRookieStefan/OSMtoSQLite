using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter
{
    public class ConvertTo
    {
        /// <summary>
        /// Create new SQLite database
        /// </summary>
        /// <param name="file">Input OSM file</param>
        /// <param name="sql">Path to target SQLite database</param>
        public static void SQLite(string file, string sql)
        {
            // Input checks
            if(!File.Exists(file))
                throw new FileNotFoundException(file);
            if (!Path.GetExtension(file).ToLower().Equals(".osm"))
                throw new InvalidOperationException("Wrong file extension");
            if (File.Exists(sql))
                throw new InvalidOperationException("SQLite database already exist");

            OSMReader.Read(file);
        }
       
    }
}
