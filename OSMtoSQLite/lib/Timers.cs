using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSMConverter.lib
{
    /// <summary>
    /// Identifiers for the timers used in the library.
    /// Each enum value represents a logical measurement that can be started and stopped by <see cref="Timers"/>.
    /// </summary>
    internal enum LibTimers
    {
        /// <summary>
        /// Time spent calculating the bounding box.
        /// </summary>
        Filter_BoundingBox,                     // Time for bounding box

        /// <summary>
        /// Time spent performing filtering (e.g. extracting buildings, highways).
        /// </summary>
        Filter_Filtering,                       // Time for extracting buildings/highway ....

        /// <summary>
        /// Time spent storing data into SQL.
        /// </summary>
        SQL_Storing,                            // Time to store all data in SQL
    }

    /// <summary>
    /// Simple timing helper that allows starting and stopping named timers defined by <see cref="LibTimers"/>.
    /// Recorded durations are stored in an internal dictionary and can be retrieved via <see cref="GetTimerValue"/>.
    /// This class is thread-unsafe and intended for simple sequential timing in the application.
    /// </summary>
    internal class Timers
    {
        /// <summary>
        /// Internal storage mapping timer name (string) to the measured <see cref="TimeSpan"/>.
        /// Keys are the <see cref="LibTimers"/> values converted to strings.
        /// </summary>
        private static Dictionary<string, TimeSpan> TimerStorage = new Dictionary<string, TimeSpan>();  

        /// <summary>
        /// The currently running timer type.
        /// </summary>
        private static LibTimers Type;

        /// <summary>
        /// UTC timestamp when the current timer was started.
        /// </summary>
        private static DateTime StartTime;

        /// <summary>
        /// Indicates whether a timer is currently running.
        /// </summary>
        private static bool TimerRunning;

        /// <summary>
        /// Start a timer for the specified <paramref name="type"/>.
        /// Records the current UTC time as the start time.
        /// </summary>
        /// <param name="type">The logical timer to start (one of <see cref="LibTimers"/>).</param>
        /// <exception cref="InvalidOperationException">Thrown when another timer is already running.</exception>
        internal static void StartTimer(LibTimers type)
        {
            if (TimerRunning)
                throw new InvalidOperationException("Timer: Timer is already running");

            StartTime = DateTime.UtcNow;
            Type = type;
            TimerRunning = true;    
        }

        /// <summary>
        /// Stop the currently running timer and store its elapsed duration.
        /// If no timer is running, this method returns immediately and does nothing.
        /// </summary>
        /// <exception cref="Exception">Thrown when the recorded timer type is unknown (should not occur for defined enum values).</exception>
        internal static void StopTimer()
        {
            // Do nothing when timer is not running
            if (!TimerRunning)
                return;

            TimerRunning = false;
            TimeSpan elapsedTime = DateTime.UtcNow - StartTime;

            switch (Type)
            {
                case LibTimers.Filter_BoundingBox:
                    StoreTime(LibTimers.Filter_BoundingBox.ToString(), elapsedTime);
                    break;
                case LibTimers.Filter_Filtering:
                    StoreTime(LibTimers.Filter_Filtering.ToString(), elapsedTime);
                    break;
                case LibTimers.SQL_Storing:
                    StoreTime(LibTimers.SQL_Storing.ToString(), elapsedTime);
                    break;
                default:
                    throw new Exception($"Timer: Unknown timer type {Type}");
            }
        }

        /// <summary>
        /// Retrieve the stored elapsed time for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The logical timer to query.</param>
        /// <returns>
        /// The recorded <see cref="TimeSpan"/> for the given timer type, or <see cref="TimeSpan.Zero"/> if no value is stored.
        /// </returns>
        internal static TimeSpan GetTimerValue(LibTimers type)
        {
            if (TimerStorage.ContainsKey(type.ToString()))
                return TimerStorage[type.ToString()];
            return new TimeSpan();
        }

        /// <summary>
        /// Store a measured elapsed time under the provided <paramref name="key"/>.
        /// Keys should be the string representation of a <see cref="LibTimers"/> value.
        /// </summary>
        /// <param name="key">Timer type as string.</param>
        /// <param name="elapsedTime">The elapsed <see cref="TimeSpan"/> to store.</param>
        /// <exception cref="InvalidOperationException">Thrown when a value for <paramref name="key"/> is already present.</exception>
        private static void StoreTime(string key, TimeSpan elapsedTime)
        {
            if(TimerStorage.ContainsKey(key))
                throw new InvalidOperationException($"Timer: Storage contains already key {key}");
            TimerStorage[key] = elapsedTime;    
        }
    }
}
