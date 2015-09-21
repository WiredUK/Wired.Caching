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

        private readonly string _name;

        /// <summary>
        /// Default constructor
        /// </summary>
        public InMemoryCache()
        {
            
        }

        /// <summary>
        /// Specift the name when creating the cache service
        /// </summary>
        /// <param name="name">The name of the cache</param>
        public InMemoryCache(string name)
        {
            _name = name;
        }

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
        private MemoryCache GetCache()
        {
            return string.IsNullOrEmpty(_name) ? 
                MemoryCache.Default : 
                new MemoryCache(_name);
        }
    }
}
