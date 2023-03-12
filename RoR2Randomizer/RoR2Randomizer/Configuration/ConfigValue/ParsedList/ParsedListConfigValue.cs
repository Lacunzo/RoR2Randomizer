using BepInEx.Configuration;
using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.Configuration.ConfigValue.ParsedList
{
    public abstract class ParsedListConfigValue<T> : StringConfigValue, IEnumerable<T>
    {
        T[] _parsedArray;
        public ReadOnlyArray<T> ParsedArray { get; private set; }

        public ParsedListConfigValue(ConfigEntry<string> entry) : base(entry)
        {
            _parsedArray = Array.Empty<T>();
            ParsedArray = _parsedArray;

            OnChange += tryParseToList;
            tryParseToList();
        }

        protected void tryParseToList()
        {
            string value = Entry.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            string[] splitValue = value.Split(',');
            if (splitValue.Length == 0)
                return;

            _parsedArray = parse(splitValue).ToArray();
            ParsedArray = _parsedArray;
        }

        protected abstract IEnumerable<T> parse(string[] values);

        public int BinarySearch(T value, IComparer<T> comparer = null)
        {
            if (comparer != null)
            {
                return Array.BinarySearch(_parsedArray, value, comparer);
            }
            else
            {
                return Array.BinarySearch(_parsedArray, value);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_parsedArray).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parsedArray.GetEnumerator();
        }
    }
}
