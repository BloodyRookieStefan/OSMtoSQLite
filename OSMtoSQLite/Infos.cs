using OSMConverter.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter
{
    public class Infos
    {
        /// <summary>
        /// Get elapsed seconds for filtering OSM files
        /// </summary>
        /// <returns>Total elapsed seconds</returns>
        public static double GetFilterElapsedSeconds()
        {
            return DataFilter.ElapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Get elapsed seconds reading OSM files
        /// </summary>
        /// <returns>Total elapsed seconds</returns>
        public static double GetOSMReaderElapsedSeconds()
        {
            return OSMReader.ElapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Get elapsed seconds reading OSM files
        /// </summary>
        /// <returns>Total elapsed seconds</returns>
        public static double GetSQLStoringElapsedSeconds()
        {
            return SQLController.ElapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Get total number of nodes
        /// </summary>
        /// <returns>Total number of OSM nodes</returns>
        public static long GetTotalNodeCount()
        {
            return OSMReader.Nodes.Count;
        }

        /// <summary>
        /// Get total number of ways
        /// </summary>
        /// <returns>Total number of OSM ways</returns>
        public static long GetTotalWayCount()
        {
            return OSMReader.Ways.Count;
        }

        /// <summary>
        /// Get total number of relations
        /// </summary>
        /// <returns>Total number of OSM relations</returns>
        public static long GetTotalRelationCount()
        {
            return OSMReader.Relations.Count;
        }
    }
}
