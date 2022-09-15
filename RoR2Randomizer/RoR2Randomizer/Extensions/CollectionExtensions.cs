using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Extensions
{
    public static class CollectionExtensions
    {
        public static T GetRandomOrDefault<T>(this IEnumerable<T> collection)
        {
            int count = collection.Count();
            if (count > 0)
            {
                return collection.ElementAt(Run.instance.runRNG.RangeInt(0, count));
            }
            else
            {
                return default;
            }
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
    }
}
