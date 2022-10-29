using HarmonyLib;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public class ReplacementDictionary<T> : ReadOnlyDictionary<T, T>
    {
        public static ReplacementDictionary<T> CreateFrom<TSrc>(IEnumerable<TSrc> collection, Func<TSrc, T> converter, Func<TSrc, TSrc, bool> canReplaceFunc)
        {
            return CreateFrom(collection, converter, canReplaceFunc, null);
        }

        public static ReplacementDictionary<T> CreateFrom<TSrc>(IEnumerable<TSrc> collection, Func<TSrc, T> converter, Func<TSrc, TSrc, bool> canReplaceFunc, Func<TSrc, TSrc, float> weightSelector)
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
                        value = availableValues.PickWeighted(v => weightSelector(key, v));
                    }
                    else
                    {
                        value = availableValues.GetRandomOrDefault();
                    }

                    valuesList.Remove(value);

                    result.Add(converter(key), converter(value));
                }
                else
                {
                    Log.Warning($"{nameof(ReplacementDictionary<T>)}<{typeof(T).Name}>.Create<{typeof(TSrc).Name}>: No valid replacement exists for key '{key}'. It will be excluded from the resulting dictionary");
                    valuesList.Remove(key);
                }
            }

            return new ReplacementDictionary<T>(result);
        }

        public static ReplacementDictionary<T> CreateFrom(IEnumerable<T> collection)
        {
            return CreateFrom(collection, canReplaceFunc: null, null);
        }

        public static ReplacementDictionary<T> CreateFrom(IEnumerable<T> collection, Func<T, T, bool> canReplaceFunc, Func<T, T, float> weightSelector)
        {
            return CreateFrom(collection, t => t, canReplaceFunc, weightSelector);
        }

        public static ReplacementDictionary<T> CreateFrom(IEnumerable<T> collection, Func<T, T, bool> canReplaceFunc)
        {
            return CreateFrom(collection, t => t, canReplaceFunc, null);
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
