using OSMConverter.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter
{
    /// <summary>
    /// Provides read-only informational helpers about the current OSM dataset and timing values.
    /// </summary>
    /// <remarks>
    /// All methods on this class return values derived from other static components:
    /// - Node, Way and Relation counts are returned from the in-memory collections on <see cref="OSMReader"/>.
    /// - Timing values are returned from <see cref="Timers.GetTimerValue"/> using the <see cref="LibTimers"/> keys.
    ///
    /// These helpers do not modify state and are intended for diagnostics, logging and UI display.
    /// Callers should be aware that the counts reflect the current in-memory state of <see cref="OSMReader"/>;
    /// if those collections are null or uninitialized a <see cref="NullReferenceException"/> may be thrown.
    /// </remarks>
    public class Infos
    {
        /// <summary>
        /// Gets the total number of OSM nodes currently tracked by <see cref="OSMReader"/>.
        /// </summary>
        /// <returns>
        /// The number of nodes as a <see cref="long"/>. This value is typically the value of
        /// <c>OSMReader.Nodes.Count</c>.
        /// </returns>
        /// <remarks>
        /// The returned value represents the in-memory count of nodes. If the node collection is
        /// not initialized this method will throw a <see cref="NullReferenceException"/>.
        /// </remarks>
        public static long GetTotalNodeCount()
        {
            return OSMReader.Nodes.Count;
        }

        /// <summary>
        /// Gets the total number of OSM ways currently tracked by <see cref="OSMReader"/>.
        /// </summary>
        /// <returns>
        /// The number of ways as a <see cref="long"/>. This value is typically the value of
        /// <c>OSMReader.Ways.Count</c>.
        /// </returns>
        /// <remarks>
        /// The returned value represents the in-memory count of ways. If the way collection is
        /// not initialized this method will throw a <see cref="NullReferenceException"/>.
        /// </remarks>
        public static long GetTotalWayCount()
        {
            return OSMReader.Ways.Count;
        }

        /// <summary>
        /// Gets the total number of OSM relations currently tracked by <see cref="OSMReader"/>.
        /// </summary>
        /// <returns>
        /// The number of relations as a <see cref="long"/>. This value is typically the value of
        /// <c>OSMReader.Relations.Count</c>.
        /// </returns>
        /// <remarks>
        /// The returned value represents the in-memory count of relations. If the relation collection is
        /// not initialized this method will throw a <see cref="NullReferenceException"/>.
        /// </remarks>
        public static long GetTotalRelationCount()
        {
            return OSMReader.Relations.Count;
        }

        /// <summary>
        /// Gets the overall elapsed time for filtering and storing operations.
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the sum of filter-related time and storing time.
        /// </returns>
        /// <remarks>
        /// This value is computed as <see cref="GetOverallFilterTime"/> + <see cref="GetStoringTime"/>.
        /// Timing values are retrieved from the <see cref="Timers"/> helper using keys defined in <see cref="LibTimers"/>.
        /// </remarks>
        public static TimeSpan GetOverallTime()
        {
            return GetOverallFilterTime() + GetStoringTime();
        }

        /// <summary>
        /// Gets the total elapsed time spent performing all filtering operations.
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> equal to the sum of general filter time and bounding-box-specific filter time.
        /// </returns>
        /// <remarks>
        /// Computed as <see cref="GetFilterTime"/> + <see cref="GetFilterTimeBoundingBox"/>.
        /// </remarks>
        public static TimeSpan GetOverallFilterTime()
        {
            return GetFilterTime() + GetFilterTimeBoundingBox();
        }

        /// <summary>
        /// Gets the elapsed time spent in the general filtering step.
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the time recorded for <see cref="LibTimers.Filter_Filtering"/>.
        /// </returns>
        public static TimeSpan GetFilterTime()
        {
            return Timers.GetTimerValue(LibTimers.Filter_Filtering);
        }

        /// <summary>
        /// Gets the elapsed time spent creating and applying the bounding box filter.
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the time recorded for <see cref="LibTimers.Filter_BoundingBox"/>.
        /// </returns>
        public static TimeSpan GetFilterTimeBoundingBox()
        {
            return Timers.GetTimerValue(LibTimers.Filter_BoundingBox);
        }

        /// <summary>
        /// Gets the elapsed time spent storing data to the output (for example, writing to the SQLite database).
        /// </summary>
        /// <returns>
        /// A <see cref="TimeSpan"/> representing the time recorded for <see cref="LibTimers.SQL_Storing"/>.
        /// </returns>
        public static TimeSpan GetStoringTime()
        {
            return Timers.GetTimerValue(LibTimers.SQL_Storing);
        }
    }
}
