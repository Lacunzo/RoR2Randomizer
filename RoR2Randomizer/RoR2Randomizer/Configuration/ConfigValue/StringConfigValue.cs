using BepInEx.Configuration;

namespace RoR2Randomizer.Configuration.ConfigValue
{
    public class StringConfigValue : GenericConfigValue<string>
    {
        public StringConfigValue(ConfigEntry<string> entry) : base(entry)
        {
        }

        public override void CreateRiskOfOptionsEntry()
        {
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Entry));
        }
    }
}
