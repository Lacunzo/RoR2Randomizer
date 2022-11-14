using BepInEx.Configuration;
using System;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public sealed class EnumConfigValue<T> : GenericConfigValue<T> where T : Enum
    {
        public EnumConfigValue(ConfigEntry<T> entry) : base(entry)
        {
        }

        public override void CreateRiskOfOptionsEntry()
        {
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(Entry));
        }
    }
}
