using BepInEx.Configuration;
using System;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public abstract class GenericConfigValue<T> : IConfigModCompatibility
    {
        public event Action OnChange;

        public readonly ConfigEntry<T> Entry;

        protected GenericConfigValue(ConfigEntry<T> entry)
        {
            Entry = entry;
            Entry.SettingChanged += Entry_SettingChanged;
        }

        ~GenericConfigValue()
        {
            if (Entry != null)
            {
                Entry.SettingChanged -= Entry_SettingChanged;
            }
        }

        void Entry_SettingChanged(object sender, EventArgs e)
        {
            OnChange?.Invoke();
        }

        public string GetSettingPath()
        {
            ConfigDefinition definition = Entry.Definition;
            return $"{definition.Section}->{definition.Key}";
        }

        public abstract void CreateRiskOfOptionsEntry();

        public static implicit operator T(GenericConfigValue<T> config)
        {
            return config.Entry.Value;
        }
    }
}
