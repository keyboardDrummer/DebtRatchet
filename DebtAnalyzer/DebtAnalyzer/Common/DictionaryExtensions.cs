using System;
using System.Collections.Generic;

namespace DebtAnalyzer
{
	public static class DictionaryExtensions
	{
		public static V Get<K, V>(this IReadOnlyDictionary<K, V> dictionary, K key, Func<V> defaultValue)
		{
			V result;
			if (dictionary.TryGetValue(key, out result))
				return result;
			return defaultValue();
		}
	}
}