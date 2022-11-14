using BepInEx.Configuration;

namespace RoR2Randomizer.Configuration.ConfigValue
{
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
