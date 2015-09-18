using System;
using System.Runtime.Caching;

namespace Wired.Caching
{
    /// <summary>
    /// Provides an in-memory cache
    /// </summary>
    public class InMemoryCache : ICacheService
    {
        private static readonly object SyncObject = new object();

        /// <summary>
        /// Gets an item from cache or calls the callback method and stores the result in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">The key of the item in teh cache</param>
        /// <param name="getItemCallback">Delegate callback to get the item if needed</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        /// <returns>The cached item or result of the callback if item is not in the cache</returns>
        public T Get<T>(string cacheKey, Func<T> getItemCallback, int duration) where T : class
        {
            lock (SyncObject)
            {
                var item = MemoryCache.Default.Get(cacheKey) as T;

                if (item != null) return item;

                item = getItemCallback();

                MemoryCache.Default.Add(cacheKey, item, DateTime.Now.AddSeconds(duration));
                return item;
            }
        }
    }
}
