using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public class ReplacementDictionary<T> : ReadOnlyDictionary<T, T>
    {
        public static ReplacementDictionary<T> CreateFrom<TSrc>(IEnumerable<TSrc> collection, Func<TSrc, T> converter, Func<TSrc, TSrc, bool> canReplaceFunc)
        {
            Dictionary<T, T> result = new Dictionary<T, T>();

            List<TSrc> keysList = new List<TSrc>(collection);
            List<TSrc> valuesList = new List<TSrc>(keysList);

            int count = keysList.Count;
            for (int i = 0; i < count; i++)
            {
                TSrc key = keysList.GetAndRemoveAt(0);

                IEnumerable<TSrc> availableValues = valuesList.Where(v => canReplaceFunc == null || canReplaceFunc(key, v));
                if (availableValues.Any())
                {
                    TSrc value = availableValues.GetRandomOrDefault();
                    valuesList.Remove(value);

                    result.Add(converter(key), converter(value));
                }
                else
                {
                    Log.Warning($"ReplacementDictionary<{typeof(T).Name}>.Create<{typeof(TSrc).Name}>: No valid replacement exists for key '{key}'. It will be excluded from the resulting dictionary");
                    valuesList.Remove(key);
                }
            }

            return new ReplacementDictionary<T>(result);
        }

        static Dictionary<T, T> baseDictFromCollection(IEnumerable<T> collection, Func<T, T, bool> canReplaceFunc)
        {
            Dictionary<T, T> result = new Dictionary<T, T>();

            List<T> keysList = new List<T>(collection);
            List<T> valuesList = new List<T>(keysList);

            int count = keysList.Count;
            for (int i = 0; i < count; i++)
            {
                T key = keysList.GetAndRemoveAt(0);

                IEnumerable<T> availableValues = valuesList.Where(v => canReplaceFunc == null || canReplaceFunc(key, v));
                if (availableValues.Any())
                {
                    T value = availableValues.GetRandomOrDefault();
                    result.Add(key, value);
                    valuesList.Remove(value);
                }
                else
                {
                    Log.Warning($"ReplacementDictionary<{typeof(T).Name}>.baseDictFromCollection: No valid replacement exists for key '{key}'. It will be excluded from the resulting dictionary");
                    valuesList.Remove(key);
                }
            }

            return result;
        }

        ReadOnlyDictionary<T, T> _reverseDictionary;
        ReadOnlyDictionary<T, T> reverseDictionary
        {
            get
            {
                if (_reverseDictionary == null)
                {
                    Dictionary<T, T> dict = new Dictionary<T, T>();

                    foreach (KeyValuePair<T, T> pair in this)
                    {
                        dict[pair.Value] = pair.Key;
                    }

                    _reverseDictionary = new ReadOnlyDictionary<T, T>(dict);
                }

                return _reverseDictionary;
            }
        }

        public ReplacementDictionary(IEnumerable<T> items) : this(items, null)
        {
        }

        public ReplacementDictionary(IEnumerable<T> items, Func<T, T, bool> canReplaceFunc) : this(baseDictFromCollection(items, canReplaceFunc))
        {
        }

        public ReplacementDictionary(IDictionary<T, T> dict) : base(dict)
        {
        }

        public bool TryGetReplacement(T original, out T replacement)
        {
            return TryGetValue(original, out replacement);
        }

        public bool TryGetOriginal(T replacement, out T original)
        {
            return reverseDictionary.TryGetValue(replacement, out original);
        }
    }
}
