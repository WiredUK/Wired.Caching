using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Wired.Caching
{
    /// <summary>
    /// Provides an in-memory cache
    /// </summary>
    public class InMemoryCache : ICacheService
    {
        //Some of the async code was taken from an excellent answer on Stackoverflow
        //See http://stackoverflow.com/a/36001954/1663001 for more detail

        private const string CacheKeyDurationSuffix = ":CacheDuration";

        private static readonly object SyncObject = new object();

        private static DateTime _lastPurge = DateTime.MinValue;

        private static readonly TimeSpan MinPurgeFrequency = TimeSpan.FromHours(1);
        private static readonly SemaphoreSlim PurgeLock = new SemaphoreSlim(1);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new ConcurrentDictionary<string, SemaphoreSlim>();

        /// <summary>
        /// Determines if the cache service also retains the cache times, allowing you to
        /// retrieve the length of time an item is due to remain in the cache.
        /// </summary>
        public bool RetainCacheDurationDetail { get; set; }

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
        /// <typeparam name="T">The return type</typeparam>
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
                if (!RetainCacheDurationDetail) return item;

                var cacheDetail = new CacheItemDetail
                {
                    AddedOn = DateTime.Now,
                    Duration = duration
                };

                cache.Add($"{key}{CacheKeyDurationSuffix}", cacheDetail, DateTime.Now.AddSeconds(duration));
                return item;

            }
        }

        /// <summary>
        /// Gets an item from the cache or calls the callback and stores the result in the cache.
        /// This is all done asynchronously.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <param name="getItemFactory">Delegate task callback to get the item if needed</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        /// <returns>A task of the cached item or result of the callback if item is not in the cache</returns>
        public Task<T> GetAsync<T>(string key, Func<Task<T>> getItemFactory, int duration) where T : class
        {
            var cache = GetCache();
            var result = (T)cache.Get(key);

            return result != null ?
                Task.FromResult(result) :
                RunFactory(cache, key, getItemFactory, duration);
        }

        /// <summary>
        /// Gets an item from the cache or calls the callback and stores the result in the cache.
        /// This is all done asynchronously.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <typeparam name="TParam">The type of the delegate parameter</typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <param name="getItemFactory">Delegate task callback to get the item if needed</param>
        /// <param name="inputParam">The input parameter to the delegate</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        /// <returns>A task of the cached item or result of the callback if item is not in the cache</returns>
        public Task<T> GetAsync<T, TParam>(string key, Func<TParam, Task<T>> getItemFactory, TParam inputParam, int duration) where T : class
        {
            var cache = GetCache();
            var result = (T)cache.Get(key);

            return result != null ?
                Task.FromResult(result) :
                RunFactory(cache, key, getItemFactory, inputParam, duration);
        }

        /// <summary>
        /// Get information about the cache item. Requires the <see cref="RetainCacheDurationDetail">RetainCacheDurationDetail</see> 
        /// property to be set to true.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CacheItemDetail GetCacheItemDetail(string key)
        {
            return ReadFromCache<CacheItemDetail>($"{key}{CacheKeyDurationSuffix}");
        }

        /// <summary>
        /// Reads an item from the cache, does not create a new item.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
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

        #region Private methods
        /// <summary>
        /// Internal method to get the correct cache
        /// </summary>
        /// <returns></returns>
        private static ObjectCache GetCache()
        {
            return MemoryCache.Default;
        }

        private async Task<T> RunFactory<T>(ObjectCache cache, string key, Func<Task<T>> getItemFactory, int duration) where T : class
        {
            await PurgeOldLocks();
            var cacheLock = Locks.GetOrAdd(key, k => new SemaphoreSlim(1));
            try
            {
                //Wait for anyone currently running the factory.
                await cacheLock.WaitAsync();

                //Check to see if another factory has already ran while we waited.
                var oldResult = (T)cache.Get(key);
                if (oldResult != null)
                {
                    return oldResult;
                }

                //Run the factory then cache the result.
                var newResult = await getItemFactory();
                cache.Add(key, newResult, DateTime.Now.AddSeconds(duration));

                if (!RetainCacheDurationDetail) return newResult;

                var cacheDetail = new CacheItemDetail
                {
                    AddedOn = DateTime.Now,
                    Duration = duration
                };

                cache.Add($"{key}{CacheKeyDurationSuffix}", cacheDetail, DateTime.Now.AddSeconds(duration));

                return newResult;
            }
            finally
            {
                cacheLock.Release();
            }
        }

        private async Task<T> RunFactory<T, TParam>(ObjectCache cache, string key, Func<TParam, Task<T>> getItemFactory, TParam inputParam, int duration) where T : class
        {
            await PurgeOldLocks();
            var cacheLock = Locks.GetOrAdd(key, k => new SemaphoreSlim(1));
            try
            {
                //Wait for anyone currently running the factory.
                await cacheLock.WaitAsync();

                //Check to see if another factory has already ran while we waited.
                var oldResult = (T)cache.Get(key);
                if (oldResult != null)
                {
                    return oldResult;
                }

                //Run the factory then cache the result.
                var newResult = await getItemFactory(inputParam);
                cache.Add(key, newResult, DateTime.Now.AddSeconds(duration));

                if (!RetainCacheDurationDetail) return newResult;

                var cacheDetail = new CacheItemDetail
                {
                    AddedOn = DateTime.Now,
                    Duration = duration
                };

                cache.Add($"{key}{CacheKeyDurationSuffix}", cacheDetail, DateTime.Now.AddSeconds(duration));

                return newResult;
            }
            finally
            {
                cacheLock.Release();
            }
        }

        private static async Task PurgeOldLocks()
        {
            try
            {
                //Only one thread can run the purge;
                await PurgeLock.WaitAsync();

                if ((DateTime.UtcNow - _lastPurge).Duration() > MinPurgeFrequency)
                {
                    _lastPurge = DateTime.UtcNow;
                    var locksSnapshot = Locks.ToList();
                    foreach (var kvp in locksSnapshot)
                    {
                        //Try to take the lock but do not wait for it.
                        var waited = await kvp.Value.WaitAsync(0);
                        if (!waited) continue;

                        //We where able to take the lock so remove it from the collection and dispose it.
                        SemaphoreSlim _;
                        Locks.TryRemove(kvp.Key, out _);
                        kvp.Value.Dispose();
                    }
                }
            }
            finally
            {
                PurgeLock.Release();
            }
        }

        #endregion

    }
}
