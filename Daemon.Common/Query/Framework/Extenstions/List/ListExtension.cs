using System;
using System.Collections.Generic;
using System.Linq;

namespace Daemon.Common.Query.Framework
{
	public static class ListExtension
	{
		public static bool Contains(this IEnumerable<string> list, string value, StringComparison stringComparison)
		{
			return list.Any(r => r.Equals(value, stringComparison));
		}

		public static IDictionary<TKey, T> SafeToDictionary<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer = null)
		{
			var result = new Dictionary<TKey, T>(keyComparer);
			foreach (var item in source)
			{
				var key = keySelector(item);
				if (result.ContainsKey(key))
				{
					continue;
				}

				result.Add(key, item);
			}

			return result;
		}

        public static IDictionary<TKey, TValue> SafeToDictionary<T, TKey, TValue>(this IEnumerable<T> source, Func<T, TKey> keySelector, Func<T, TValue> valueSelector, IEqualityComparer<TKey> keyComparer = null)
        {
            var result = new Dictionary<TKey, TValue>(keyComparer);
            foreach (var item in source)
            {
                var key = keySelector(item);
                if (result.ContainsKey(key))
                {
                    continue;
                }

                var value = valueSelector(item);
                result.Add(key, value);
            }

            return result;
        }
	}
}
