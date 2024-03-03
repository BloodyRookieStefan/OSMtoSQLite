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

            Console.WriteLine($"Time overall: {(int)(OSMConverter.Infos.GetOverallTime().TotalSeconds/ 60)} min - {OSMConverter.Infos.GetOverallTime().TotalSeconds % 60} sec");
            Console.WriteLine($"Time overall filtering: {(int)(OSMConverter.Infos.GetOverallFilterTime().TotalSeconds / 60)} min - {OSMConverter.Infos.GetOverallFilterTime().TotalSeconds % 60} sec");

            Console.WriteLine($"Time bounding box: {(int)(OSMConverter.Infos.GetFilterTimeBoundingBox().TotalSeconds / 60)} min - {OSMConverter.Infos.GetFilterTimeBoundingBox().TotalSeconds % 60} sec");
            Console.WriteLine($"Time filter: {(int)(OSMConverter.Infos.GetFilterTime().TotalSeconds / 60)} min - {OSMConverter.Infos.GetFilterTime().TotalSeconds    % 60} sec");
            Console.WriteLine($"Time for storing: {(int)(OSMConverter.Infos.GetStoringTime().TotalSeconds / 60)} min - {OSMConverter.Infos.GetStoringTime().TotalSeconds     % 60} sec");

            Console.WriteLine($"Number of nodes: {OSMConverter.Infos.GetTotalNodeCount()}");
            Console.WriteLine($"Number of ways: {OSMConverter.Infos.GetTotalWayCount()}");
            Console.WriteLine($"Number of relations: {OSMConverter.Infos.GetTotalRelationCount()}");

            Console.WriteLine($"---- Press any key ----");
            Console.ReadLine();
        }
    }
}
