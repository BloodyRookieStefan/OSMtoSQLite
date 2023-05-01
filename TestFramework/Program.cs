using OSMConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Point x = new Point(48.9850614, 9.2147826);
            Point y = new Point(48.935751, 9.3031024);

            OSMConverter.Convert.BZ2toSQLite("E:\\GitHub\\OSMtoSQLite\\DebugData\\stuttgart-regbez-latest.osm.bz2",
                                             "E:\\GitHub\\OSMtoSQLite\\SQLDatabase",
                                             new List<Filter> { Filter.Buildings, Filter.Highway, Filter.Railway, Filter.Wood, Filter.Waterway },
                                             x, y);

            Console.WriteLine($"Time filtering: {(int)(OSMConverter.Infos.GetFilterElapsedSeconds() / 60)} min - {OSMConverter.Infos.GetFilterElapsedSeconds() % 60} sec");
            Console.WriteLine($"Time OSM reading: {(int)(OSMConverter.Infos.GetOSMReaderElapsedSeconds() / 60)} min - {OSMConverter.Infos.GetOSMReaderElapsedSeconds() % 60} sec");
            Console.WriteLine($"Time SQL storing: {(int)(OSMConverter.Infos.GetSQLStoringElapsedSeconds() / 60)} min - {OSMConverter.Infos.GetSQLStoringElapsedSeconds() % 60} sec");

            Console.WriteLine($"Number of nodes: {OSMConverter.Infos.GetTotalNodeCount()}");
            Console.WriteLine($"Number of ways: {OSMConverter.Infos.GetTotalWayCount()}");
            Console.WriteLine($"Number of relations: {OSMConverter.Infos.GetTotalRelationCount()}");

            Console.WriteLine($"---- Press any key ----");
            Console.ReadLine();
        }
    }
}
