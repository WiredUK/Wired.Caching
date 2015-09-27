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
            return GetCache().Contains(key);
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
                var cache = GetCache();

                var item = cache.Get(key) as T;

                if (item != null) return item;

                item = getItemDelegate();

                cache.Add(key, item, DateTime.Now.AddSeconds(duration));
                return item;
            }
        }
        
        /// <summary>
        /// Reads an item from the cache, does not create a new item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <returns>The cached item</returns>
        public T ReadFromCache<T>(string key) where T : class
        {
            lock (SyncObject)
            {
                var cache = GetCache();
                return cache.Get(key) as T;
            }
        }

        /// <summary>
        /// Inserts an item into the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <param name="item">The item to cache</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        public void InsertIntoCache<T>(string key, T item, int duration) where T : class
        {
            var cache = GetCache();
            cache.Add(key, item, DateTime.Now.AddSeconds(duration));
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">The key of the item in the cache</param>
        public void RemoveFromCache(string key)
        {
            GetCache().Remove(key);
        }

        /// <summary>
        /// Internal method to get the correct cache
        /// </summary>
        /// <returns></returns>
        private static MemoryCache GetCache()
        {
            return MemoryCache.Default;
        }
    }
}
