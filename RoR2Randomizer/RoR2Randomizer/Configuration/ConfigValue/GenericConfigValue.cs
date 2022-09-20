using BepInEx.Configuration;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public abstract class GenericConfigValue<T> : IConfigModCompatibility
    {
        public readonly ConfigEntry<T> Entry;

        protected GenericConfigValue(ConfigEntry<T> entry)
        {
            Entry = entry;
        }

        public abstract void CreateRiskOfOptionsEntry();

        public static implicit operator T(GenericConfigValue<T> config)
        {
            return config.Entry.Value;
        }
    }
}
