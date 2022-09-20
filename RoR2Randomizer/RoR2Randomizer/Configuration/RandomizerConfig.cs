using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.Configuration
{
    public abstract class BaseRandomizerConfig : ConfigCategory
    {
        public readonly BoolConfigValue Enabled;

        protected BaseRandomizerConfig(string randomizerName, ConfigFile file) : base(randomizerName, file)
        {
            Enabled = new BoolConfigValue(getEntry<bool>($"Enabled", new ConfigDescription("If the randomizer should be enabled"), true));
        }
    }
}
