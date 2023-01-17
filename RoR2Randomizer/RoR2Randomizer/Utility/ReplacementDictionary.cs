using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public class ReplacementDictionary<T> : ReadOnlyDictionary<T, T>
    {
        public static ReplacementDictionary<T> CreateFrom<TSrc>(IEnumerable<TSrc> collection, Xoroshiro128Plus rng, Func<TSrc, T> converter, Func<TSrc, TSrc, bool> canReplaceFunc)
        {
            return CreateFrom(collection, rng, converter, canReplaceFunc, null);
        }

        public static ReplacementDictionary<T> CreateFrom<TSrc>(IEnumerable<TSrc> collection, Xoroshiro128Plus rng, Func<TSrc, T> converter, Func<TSrc, TSrc, bool> canReplaceFunc, Func<TSrc, TSrc, float> weightSelector)
        {
            Dictionary<T, T> result = new Dictionary<T, T>();

            List<TSrc> keysList = new List<TSrc>(collection);
            List<TSrc> valuesList = new List<TSrc>(keysList);

            bool hasReplaceFunc = canReplaceFunc != null;
            bool hasWeightSelector = weightSelector != null;

            while (keysList.Count > 0)
            {
                TSrc key = keysList.GetAndRemoveAt(0);

                IEnumerable<TSrc> availableValues = hasReplaceFunc ? valuesList.Where(v => canReplaceFunc(key, v)) : valuesList;
                if (availableValues.Any())
                {
                    TSrc value;
                    if (hasWeightSelector)
                    {
                        value = availableValues.PickWeighted(rng, v => weightSelector(key, v));
                    }
                    else
                    {
                        value = availableValues.GetRandomOrDefault(rng);
                    }

                    valuesList.Remove(value);

                    result.Add(converter(key), converter(value));
                }
                else
                {
                    Log.Warning($"({nameof(T)}={typeof(T).Name}, {nameof(TSrc)}={typeof(TSrc).Name}) No valid replacement exists for key '{key}'. It will be excluded from the resulting dictionary");
                    valuesList.Remove(key);
                }
            }

            return new ReplacementDictionary<T>(result);
        }

        public static ReplacementDictionary<T> CreateFrom(IEnumerable<T> collection, Xoroshiro128Plus rng)
        {
            return CreateFrom(collection, rng, canReplaceFunc: null, null);
        }

        public static ReplacementDictionary<T> CreateFrom(IEnumerable<T> collection, Xoroshiro128Plus rng, Func<T, T, bool> canReplaceFunc, Func<T, T, float> weightSelector)
        {
            return CreateFrom(collection, rng, t => t, canReplaceFunc, weightSelector);
        }

        public static ReplacementDictionary<T> CreateFrom(IEnumerable<T> collection, Xoroshiro128Plus rng, Func<T, T, bool> canReplaceFunc)
        {
            return CreateFrom(collection, rng, t => t, canReplaceFunc, null);
        }

        readonly InitializeOnAccess<ReadOnlyDictionary<T, T>> _reverseDictionary;

        public ReplacementDictionary(IDictionary<T, T> dict) : base(dict)
        {
            _reverseDictionary = new InitializeOnAccess<ReadOnlyDictionary<T, T>>(() =>
            {
                Dictionary<T, T> dict = new Dictionary<T, T>();

                foreach (KeyValuePair<T, T> pair in this)
                {
                    dict[pair.Value] = pair.Key;
                }

                return new ReadOnlyDictionary<T, T>(dict);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetReplacement(T original, out T replacement)
        {
            return TryGetValue(original, out replacement);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasReplacement(T original)
        {
            return ContainsKey(original);
        }

        public bool TryGetOriginal(T replacement, out T original)
        {
            return _reverseDictionary.Get.TryGetValue(replacement, out original);
        }
    }
}
