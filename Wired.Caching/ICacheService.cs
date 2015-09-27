﻿using System;

namespace Wired.Caching
{
    /// <summary>
    /// Interface to a cache service, useful for injecting a cache into your app
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Determines if an item is in the cache by key
        /// </summary>
        /// <param name="key">The key of the item in the cache</param>
        /// <returns></returns>
        bool IsInCache(string key);

        /// <summary>
        /// Gets an item from cache or calls the callback methos and stores the result in the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <param name="getItemDelegate">Delegate callback to get the item if needed</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        /// <returns>The cached item or result of the callback if item is not in the cache</returns>
        T Get<T>(string key, Func<T> getItemDelegate, int duration) where T : class;

        /// <summary>
        /// Reads an item from the cache, does not create a new item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <returns>The cached item</returns>
        T ReadFromCache<T>(string key) where T : class;

        /// <summary>
        /// Inserts an item into the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key of the item in the cache</param>
        /// <param name="item">The item to cache</param>
        /// <param name="duration">The duration in seconds to store the item in the cache</param>
        void InsertIntoCache<T>(string key, T item, int duration) where T : class;

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">The key of the item in the cache</param>
        void RemoveFromCache(string key);
    }
}
