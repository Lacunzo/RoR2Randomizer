#if !DISABLE_ITEM_RANDOMIZER
using BepInEx.Configuration;
using RoR2Randomizer.Configuration;

namespace RoR2Randomizer.RandomizerControllers.Item
{
    public sealed class ItemRandomizerConfig : BaseRandomizerConfig
    {
        public ItemRandomizerConfig(ConfigFile file) : base("Item", file)
        {
        }
    }
}
#endif