using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Utils.Extensions
{
	public static class DictionaryExtensions
	{
        public static TValue GetValueOrDefault<[MustBeVariant] TKey, [MustBeVariant] TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default
        )
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static void SafeAdd<[MustBeVariant] TKey, [MustBeVariant] TValue>(
            this Dictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value
        )
        {
            if (!dictionary.ContainsKey(key)) dictionary.Remove(key);
            dictionary.Add(key, value);
        }
    }
}

