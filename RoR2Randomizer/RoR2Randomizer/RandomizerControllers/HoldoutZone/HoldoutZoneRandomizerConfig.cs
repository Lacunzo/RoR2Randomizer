#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using BepInEx.Configuration;
using RoR2Randomizer.Configuration;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public sealed class HoldoutZoneRandomizerConfig : BaseRandomizerConfig
    {
        public HoldoutZoneRandomizerConfig(ConfigFile file) : base("Holdout Zone", file)
        {
        }
    }
}
#endif