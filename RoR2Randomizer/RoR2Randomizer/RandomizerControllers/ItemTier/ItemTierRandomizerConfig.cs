using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.RandomizerControllers.Item_Tier
{
    public sealed class ItemTierRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue RandomizeScrap;

        public ItemTierRandomizerConfig(ConfigFile file) : base("Item Tier", file)
        {
            RandomizeScrap = new BoolConfigValue(getEntry("Randomize Scrap", new ConfigDescription("If the tiers of scrap items should be randomized"), true));
        }
    }
}
