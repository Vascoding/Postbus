using System.Collections.Generic;
using System.Threading.Tasks;

namespace Postbus.Internals.Extentions
{
    public static class DictionaryExtensions
    {
        public static async Task<bool> TryAddAsync<K, V>(this IDictionary<K, V> dict, K key, V value)
        {
            return await Task.Run(() =>
            {
                if (dict.ContainsKey(key))
                {
                    return false;
                }

                dict.Add(key, value);

                return true;
            });
        }

        public static async Task<bool> TryUpdateAsync<K, V>(this IDictionary<K, V> dict, K key, V value)
        {
            return await Task.Run(() =>
            {
                if (!dict.ContainsKey(key))
                {
                    return false;
                }

                dict[key] = value;

                return true;
            });
        }

        public static async Task<bool> TryRemoveAsync<K, V>(this IDictionary<K, V> dict, K key)
        {
            return await Task.Run(() =>
            {
                if (!dict.ContainsKey(key))
                {
                    return false;
                }

                return dict.Remove(key);
            });
        }
    }
}
