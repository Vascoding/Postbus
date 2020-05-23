using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Internals.Extentions
{
    public static class CollectionExtensions
    {
        public static bool Add<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value) =>
            dict.TryAdd(key, value);

        public static async Task<bool> TryAddAsync<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value) =>
            await Task.Run(() => dict.TryAdd(key, value));

        public static async Task<bool> TryUpdateAsync<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value)
        {
            dict.TryGetValue(key, out var comparisonValue);

            return await Task.Run(() => dict.TryUpdate(key, value, comparisonValue));
        }

        public static async Task<bool> TryRemoveAsync<K, V>(this ConcurrentDictionary<K, V> dict, K key) =>
            await Task.Run(() => dict.TryRemove(key, out _));

        public static async Task<V> TryGetValueAsync<K, V>(this ConcurrentDictionary<K, V> dict, K key) =>
            await Task.Run(() => 
            {
                dict.TryGetValue(key, out var value);
                return value;
            });

        public static async Task<V> AddOrUpdateAsync<K, V>(this ConcurrentDictionary<K, V> dict, K key, V value, Func<K, V, V> updateValueFactory) =>
            await Task.Run(() => dict.AddOrUpdate(key, value, updateValueFactory));
    }
}