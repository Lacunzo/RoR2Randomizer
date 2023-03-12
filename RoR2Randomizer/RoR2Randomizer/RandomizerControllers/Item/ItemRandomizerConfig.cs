#if !DISABLE_ITEM_RANDOMIZER
using BepInEx.Configuration;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue.ParsedList;
using RoR2Randomizer.Utility.Comparers;

namespace RoR2Randomizer.RandomizerControllers.Item
{
    public sealed class ItemRandomizerConfig : BaseRandomizerConfig
    {
        readonly ParsedPickupIndexListConfigValue _pickupBlacklist;

        public bool IsBlacklisted(PickupIndex pickupIndex)
        {
            return _pickupBlacklist.BinarySearch(pickupIndex, PickupIndexComparer.Instance) >= 0;
        }

        public ItemRandomizerConfig(ConfigFile file) : base("Item", file)
        {
            _pickupBlacklist = new ParsedPickupIndexListConfigValue(getEntry("Item Blacklist", "A comma separated list of items or equipment to exclude from the item randomizer\n\nThe internal names are used for this field", string.Empty));
        }
    }
}
#endif