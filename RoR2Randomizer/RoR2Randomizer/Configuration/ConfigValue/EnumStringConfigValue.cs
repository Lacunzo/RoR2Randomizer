using BepInEx.Configuration;
using System;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public sealed class EnumStringConfigValue<T> : ParsedStringConfigValue<T> where T : struct
    {
        readonly bool _ignoreCase;

        public EnumStringConfigValue(ConfigEntry<string> entry, T fallback, bool ignoreCase) : base(entry, fallback)
        {
            _ignoreCase = ignoreCase;
        }

        protected override bool tryParse(string str, out T value)
        {
            return Enum.TryParse(str, _ignoreCase, out value);
        }
    }
}
