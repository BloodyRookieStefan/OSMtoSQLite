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
        /// Get elapsed seconds for conversion
        /// </summary>
        /// <returns>Total elapsed seconds</returns>
        public static double GetTotalElapsedSeconds()
        {
            return OSMReader.ElapsedTime.TotalSeconds;
        }

        /// <summary>
        /// Get total number of nodes
        /// </summary>
        /// <returns>Total number of OSM nodes</returns>
        public static long GetTotalNodeCount()
        {
            return OSMReader.TotalNodes;
        }

        /// <summary>
        /// Get total number of ways
        /// </summary>
        /// <returns>Total number of OSM ways</returns>
        public static long GetTotalWayCount()
        {
            return OSMReader.TotalWays;
        }

        /// <summary>
        /// Get total number of relations
        /// </summary>
        /// <returns>Total number of OSM relations</returns>
        public static long GetTotalRelationCount()
        {
            return OSMReader.TotalRelations;
        }
    }
}
