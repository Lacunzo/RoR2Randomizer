using BepInEx.Configuration;

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
}
