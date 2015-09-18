using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wired.Caching.Sample
{
    public class Program
    {
        private static InMemoryCache _cacheService;

        public static void Main()
        {
            Console.WriteLine("Caching demo");
            Console.WriteLine("============");
            Console.WriteLine();

            _cacheService = new InMemoryCache();

            Console.WriteLine("First time, this should be slooooow");
            CacheDemo();

            Console.WriteLine("Second time, this should be fast!");
            CacheDemo();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void CacheDemo()
        {
            Console.Write("Getting item... ");

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var someObject = _cacheService.Get(
                "LargeObjectKey",
                () => GetObjectFromSomewhereSlowly(),
                600);

            //Alternative syntax
            //var someObject2 = _cacheService.Get(
            //    "LargeObjectKey",
            //    GetObjectFromSomewhereSlowly,
            //    600);

            Console.WriteLine("That took {0} milliseconds to get", stopWatch.ElapsedMilliseconds);
            Console.WriteLine();

        }

        private static SomeLargeObject GetObjectFromSomewhereSlowly()
        {
            //This will take 5 seconds to complete
            System.Threading.Thread.Sleep(5000);
            return new SomeLargeObject();
        }
    }
}
