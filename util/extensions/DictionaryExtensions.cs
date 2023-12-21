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
    }
}

