using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Extensions
{
    public static class CollectionExtensions
    {
        public static T GetRandomOrDefault<T>(this IEnumerable<T> enumerable, Xoroshiro128Plus rng, T fallback = default)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            if (rng is null)
                throw new ArgumentNullException(nameof(rng));

            int count;
            if (enumerable is T[] array)
            {
                count = array.Length;
                if (count > 0)
                    return rng.NextElementUniform(array);
            }
            else if (enumerable is IList<T> list)
            {
                count = list.Count;
                if (count > 0)
                    return rng.NextElementUniform(list);
            }
            else
            {
                count = enumerable.Count();

                if (count > 0)
                    return enumerable.ElementAt(rng.RangeInt(0, count));
            }

            return fallback;
        }

        public static T PickWeighted<T>(this IEnumerable<T> enumerable, Xoroshiro128Plus rng, Func<T, float> weightSelector)
        {
            if (enumerable is null)
                throw new ArgumentNullException(nameof(enumerable));

            if (rng is null)
                throw new ArgumentNullException(nameof(rng));

            if (weightSelector is null)
                throw new ArgumentNullException(nameof(weightSelector));

            int capacity;
            if (enumerable is T[] array)
            {
                capacity = array.Length;
            }
            else
            {
                capacity = enumerable.Count();
            }

            WeightedSelection<T> weightedSelection = new WeightedSelection<T>(capacity);
            foreach (T item in enumerable)
            {
                weightedSelection.AddChoice(item, weightSelector(item));
            }

            return weightedSelection.Evaluate(rng.nextNormalizedFloat);
        }

        public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            if (dict is null)
                throw new ArgumentNullException(nameof(dict));

            if (!dict.TryGetValue(key, out TValue value))
            {
                value = new TValue();
                dict.Add(key, value);
            }

            return value;
        }

        public static T GetAndRemoveAt<T>(this IList<T> list, int index)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));

            T result = list[index];
            list.RemoveAt(index);
            return result;
        }

        public static T GetAndRemoveRandom<T>(this IList<T> list, Xoroshiro128Plus rng)
        {
            return list.GetAndRemoveAt(rng.RangeInt(0, list.Count));
        }
    }
}
