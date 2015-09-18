using System;

namespace Wired.Caching
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets an item from cache or calls the callback methos and stores the result in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey">The key of the item in teh cache</param>
        /// <param name="getItemCallback">Delegate callback to get the item if needed</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        /// <returns>The cached item or result of the callback if item is not in the cache</returns>
        T Get<T>(string cacheKey, Func<T> getItemCallback, int duration) where T : class;
    }
}
