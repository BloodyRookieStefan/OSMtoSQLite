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

        /// <summary>
        /// Get overall time for all operations
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public static TimeSpan GetOverallTime()
        {
            return GetOverallFilterTime() + GetStoringTime();
        }

        /// <summary>
        /// Get overall time for filtering data
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public static TimeSpan GetOverallFilterTime()
        {
            return GetFilterTime() + GetFilterTimeBoundingBox();
        }

        /// <summary>
        /// Get time for filtering data
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public static TimeSpan GetFilterTime()
        {
            return Timers.GetTimerValue(LibTimers.Filter_Filtering);
        }

        /// <summary>
        /// Get time for creating bounding box
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public static TimeSpan GetFilterTimeBoundingBox()
        {
            return Timers.GetTimerValue(LibTimers.Filter_BoundingBox);
        }

        /// <summary>
        /// Get time for storing data
        /// </summary>
        /// <returns>Total time in seconds</returns>
        public static TimeSpan GetStoringTime()
        {
            return Timers.GetTimerValue(LibTimers.SQL_Storing);
        }
    }
}
