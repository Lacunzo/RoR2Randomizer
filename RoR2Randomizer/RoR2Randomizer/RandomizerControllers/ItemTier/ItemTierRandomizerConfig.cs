using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.RandomizerControllers.Item_Tier
{
    public sealed class ItemTierRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue ExcludeScrap;

        public ItemTierRandomizerConfig(ConfigFile file) : base("Item Tier", file)
        {
            ExcludeScrap = new BoolConfigValue(getEntry("Exclude Scrap", new ConfigDescription("If the item tiers of scrap should not be randomized"), false));
        }
    }
}
