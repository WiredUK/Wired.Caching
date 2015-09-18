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
        /// Determines if an item is in the cache by key
        /// </summary>
        /// <param name="key">The key of the item in the cache</param>
        /// <returns></returns>
        public bool IsInCache(string key)
        {
            return MemoryCache.Default.Contains(key);
        }

        /// <summary>
        /// Gets an item from cache or calls the callback method and stores the result in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <param name="getItemDelegate">Delegate callback to get the item if needed</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        /// <returns>The cached item or result of the callback if item is not in the cache</returns>
        public T Get<T>(string key, Func<T> getItemDelegate, int duration) where T : class
        {
            
            lock (SyncObject)
            {
                var item = MemoryCache.Default.Get(key) as T;

                if (item != null) return item;

                item = getItemDelegate();

                MemoryCache.Default.Add(key, item, DateTime.Now.AddSeconds(duration));
                return item;
            }
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">The key of the item in the cache</param>
        public void RemoveFromCache(string key)
        {
            MemoryCache.Default.Remove(key);
        }
    }
}
