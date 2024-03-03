using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSMConverter.lib
{
    internal enum LibTimers
    {
        Filter_BoundingBox,                     // Time for bounding box
        Filter_Filtering,                       // Time for extracting buildings/highway ....

        SQL_Storing,                            // Time to store all data in SQL
    }

    internal class Timers
    {
        private static Dictionary<string, TimeSpan> TimerStorage = new Dictionary<string, TimeSpan>();  

        private static LibTimers Type;
        private static DateTime StartTime;
        private static bool TimerRunning;

        /// <summary>
        /// Start timer
        /// </summary>
        /// <param name="type">Timer type</param>
        /// <exception cref="InvalidOperationException">Another timer is already running</exception>
        internal static void StartTimer(LibTimers type)
        {
            if (TimerRunning)
                throw new InvalidOperationException("Timer: Timer is already running");

            StartTime = DateTime.UtcNow;
            Type = type;
            TimerRunning = true;    
        }

        /// <summary>
        /// Stop running timer
        /// </summary>
        /// <exception cref="Exception">Unknown timer type</exception>
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
        /// Get timer value
        /// </summary>
        /// <param name="type">Timer type</param>
        /// <returns>Total elapsed time</returns>
        internal static TimeSpan GetTimerValue(LibTimers type)
        {
            if (TimerStorage.ContainsKey(type.ToString()))
                return TimerStorage[type.ToString()];
            return new TimeSpan();
        }

        /// <summary>
        /// Store a time value
        /// </summary>
        /// <param name="key">Timer type as string</param>
        /// <param name="elapsedTime">Timespan which elapsed</param>
        /// <exception cref="InvalidOperationException">Double entry of a timer</exception>
        private static void StoreTime(string key, TimeSpan elapsedTime)
        {
            if(TimerStorage.ContainsKey(key))
                throw new InvalidOperationException($"Timer: Storage contains already key {key}");
            TimerStorage[key] = elapsedTime;    
        }
    }
}
