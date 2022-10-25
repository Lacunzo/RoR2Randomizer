using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public abstract class ParsedStringConfigValue<T> : StringConfigValue
    {
        protected readonly T _fallback;

        protected ParsedStringConfigValue(ConfigEntry<string> entry, T fallback) : base(entry)
        {
            _fallback = fallback;
        }

        public T Parsed
        {
            get
            {
                if (tryParse(Entry.Value, out T parsedValue))
                {
                    return parsedValue;
                }
                else
                {
                    Log.Warning($"Setting {GetSettingPath()} does not have a valid value");
                    return _fallback;
                }
            }
        }

        protected abstract bool tryParse(string str, out T value);
    }

    public sealed class IntStringConfigValue : ParsedStringConfigValue<int>
    {
        public IntStringConfigValue(ConfigEntry<string> entry, int fallback) : base(entry, fallback)
        {
        }

        protected override bool tryParse(string str, out int value)
        {
            return int.TryParse(str, out value);
        }
    }
}
