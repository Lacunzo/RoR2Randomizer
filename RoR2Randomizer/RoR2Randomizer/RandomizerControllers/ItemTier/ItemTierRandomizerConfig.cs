using BepInEx.Configuration;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.Configuration.ConfigValue.ParsedList;
using RoR2Randomizer.Utility.Comparers;

namespace RoR2Randomizer.RandomizerControllers.Item_Tier
{
    public sealed class ItemTierRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue RandomizeScrap;

        readonly ParsedItemIndexListConfigValue _itemBlacklist;

        public bool IsBlacklisted(ItemIndex itemIndex)
        {
            return _itemBlacklist.BinarySearch(itemIndex) >= 0;
        }

        public ItemTierRandomizerConfig(ConfigFile file) : base("Item Tier", file)
        {
            RandomizeScrap = new BoolConfigValue(getEntry("Randomize Scrap", new ConfigDescription("If the tiers of scrap items should be randomized"), true));

            _itemBlacklist = new ParsedItemIndexListConfigValue(getEntry("Item Blacklist", "A comma separated list of items to exclude from the item tier randomizer.\n\nThe internal names are used for this field", string.Empty));
        }
    }
}
