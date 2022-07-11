using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMConverter.lib
{
    public enum Filter
    {
        None,
        Buildings,
        Highway,
        Wood,
        Waterway,
    }

    struct ComboundBoxPoint
    {
        public string Point { get; set; }    
        public string Shift { get; set; }   
    }

    internal class Decompress
    {

        internal static string[] Extract(string source, ComboundBoxPoint? top, ComboundBoxPoint? bottom, Filter[] filters)
        {
            string[] retVal = new string[5];

            retVal[0] = source;

            // If bounding box is given cut first
            if (top != null && bottom != null)
            {
                string newSource = Path.Combine(TempFolder.GetTempFolder(), "BoundingBox.osm");
                Start($"--read-xml file=\"{source}\" --bounding-box top={top.Value.Point} left={top.Value.Shift} bottom={bottom.Value.Point} right={bottom.Value.Shift} --write-xml file=\"{newSource}\"");
                retVal[0] = newSource;
            }

            // Run trough filters
            foreach(var f in filters)
            {
                string outPath = null;

                switch (f)
                {
                    case Filter.None:
                        /* Do nothing */
                        break;
                    case Filter.Buildings:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Highways.osm");
                        Start($"--read-xml file=\"{retVal[0]}\" --tf accept-ways highway=* --used-node --write-xml file=\"{outPath}\"");
                        break;
                    case Filter.Highway:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Buildings.osm");
                        Start($"--read-xml file=\"{retVal[0]}\" --tf accept-ways building=* --used-node --write-xml file=\"{outPath}\"");
                        break;
                    case Filter.Wood:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Wood.osm");
                        Start($"--read-xml file=\"{retVal[0]}\" --tf accept-ways wood=* --used-node --write-xml file=\"{outPath}\"");
                        break;
                    case Filter.Waterway:
                        outPath = Path.Combine(TempFolder.GetTempFolder(), "Waterway.osm");
                        Start($"--read-xml file=\"{retVal[0]}\" --tf accept-ways waterway=* --used-node --write-xml file=\"{outPath}\"");
                        break;
                    default:
                        throw new Exception("Invalid filter");
                        
                }
            }

            return null;
        }

        private static void Start(string command)
        {
            // Get Osmosis path
            string osmosis = GetPathOSMOSIS();
            if (osmosis == null)
                throw new Exception("Osmosis not found");

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.FileName = "cmd.exe";
            startInfo.WorkingDirectory = Path.GetDirectoryName(osmosis);
            startInfo.Arguments = $"/C osmosis {command}";
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();
        }

        /// <summary>
        /// Get path to OSMOSIS
        /// </summary>
        /// <returns>Return null if not located</returns>
        private static string GetPathOSMOSIS()
        {
            string location = Path.Combine(Dependencies.AssemblyDirectory, @"Osmosis\bin\osmosis");

            if (File.Exists(location))
                return location;

            return null;
        }
    }
}
