using System;
using System.Threading.Tasks;

namespace Wired.Caching.Sample.BasicConsole
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

            Console.WriteLine("Asynchronous Caching demo");
            Console.WriteLine("=========================");
            Console.WriteLine();

            _cacheService = new InMemoryCache();

            Console.WriteLine("First time, this should be slooooow");
            CacheDemoAsync().Wait();

            Console.WriteLine("Second time, this should be fast!");
            CacheDemoAsync().Wait();

            Console.WriteLine("Asynchronous Caching demo with a parameter");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            _cacheService = new InMemoryCache();

            Console.WriteLine("First time, this should be slooooow");
            ParameterisedCacheDemoAsync().Wait();

            Console.WriteLine("Second time, this should be fast!");
            ParameterisedCacheDemoAsync().Wait();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void CacheDemo()
        {
            Console.Write("Getting item... ");

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            _cacheService.Get(
                "LargeObjectKey",
                GetObjectFromSomewhereSlowly,
                600);

            var detail = _cacheService.GetCacheItemDetail("LargeObjectKey");

            if (detail != null)
            {
                Console.WriteLine("Item is set to expire on: {0}", detail.ExpiresOn);
            }

            Console.WriteLine("That took {0} milliseconds to get", stopWatch.ElapsedMilliseconds);
            Console.WriteLine();

        }

        private static SomeLargeObject GetObjectFromSomewhereSlowly()
        {
            //This will take 5 seconds to complete
            System.Threading.Thread.Sleep(5000);
            return new SomeLargeObject();
        }

        private static async Task<SomeLargeObject> GetObjectFromSomewhereSlowlyAsync()
        {
            //This will take 5 seconds to complete
            await Task.Delay(5000);
            return new SomeLargeObject();
        }

        private static async Task<SomeLargeObject> GetObjectFromSomewhereSlowlyWithParameterAsync(int delay)
        {
            //This will take 5 seconds to complete
            await Task.Delay(delay);
            return new SomeLargeObject();
        }

        #region Async Demos
        private static async Task CacheDemoAsync()
        {
            Console.Write("Getting item... ");

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            await _cacheService.GetAsync(
                "LargeObjectKeyAsync",
                GetObjectFromSomewhereSlowlyAsync,
                600);

            var detail = _cacheService.GetCacheItemDetail("LargeObjectKeyAsync");

            if (detail != null)
            {
                Console.WriteLine("Item is set to expire on: {0}", detail.ExpiresOn);
            }

            Console.WriteLine("That took {0} milliseconds to get", stopWatch.ElapsedMilliseconds);
            Console.WriteLine();

        }

        private static async Task ParameterisedCacheDemoAsync()
        {
            Console.Write("Getting item... ");

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            await _cacheService.GetAsync(
                "ParameterisedLargeObjectKeyAsync",
                GetObjectFromSomewhereSlowlyWithParameterAsync,
                5000,
                600);

            var detail = _cacheService.GetCacheItemDetail("ParameterisedLargeObjectKeyAsync");

            if (detail != null)
            {
                Console.WriteLine("Item is set to expire on: {0}", detail.ExpiresOn);
            }

            Console.WriteLine("That took {0} milliseconds to get", stopWatch.ElapsedMilliseconds);
            Console.WriteLine();

        }
        #endregion
    }
}
