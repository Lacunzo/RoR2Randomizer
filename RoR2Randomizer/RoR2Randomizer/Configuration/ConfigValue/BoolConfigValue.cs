using BepInEx.Configuration;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public sealed class BoolConfigValue : GenericConfigValue<bool>
    {
        public BoolConfigValue(ConfigEntry<bool> entry) : base(entry)
        {
        }

        public override void CreateRiskOfOptionsEntry()
        {
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(Entry));
        }
    }
}
