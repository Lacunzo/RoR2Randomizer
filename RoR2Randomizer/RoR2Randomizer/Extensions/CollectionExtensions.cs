using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Extensions
{
    public static class CollectionExtensions
    {
        public static T GetRandomOrDefault<T>(this IEnumerable<T> enumerable, T fallback = default)
        {
            int count;
            if (enumerable is T[] array)
            {
                count = array.Length;
                if (count > 0)
                    return array[RNGUtils.RangeInt(0, count)];
            }
            else if (enumerable is IList<T> list)
            {
                count = list.Count;
                if (count > 0)
                    return list[RNGUtils.RangeInt(0, count)];
            }
            else
            {
                if (enumerable is ICollection<T> collection)
                {
                    count = collection.Count;
                }
                else
                {
                    count = enumerable.Count();
                }

                if (count > 0)
                    return enumerable.ElementAt(RNGUtils.RangeInt(0, count));
            }

            return fallback;
        }

        public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (!dict.TryGetValue(key, out TValue value))
            {
                value = new TValue();
                dict.Add(key, value);
            }

            return value;
        }

        public static T GetAndRemoveRandom<T>(this IList<T> list)
        {
            return list.GetAndRemoveAt(RNGUtils.RangeInt(0, list.Count));
        }

        public static T GetAndRemoveAt<T>(this IList<T> list, int index)
        {
            T result = list[index];
            list.RemoveAt(index);
            return result;
        }
    }
}
