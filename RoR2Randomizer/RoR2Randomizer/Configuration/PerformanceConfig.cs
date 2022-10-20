using BepInEx.Configuration;
using RoR2Randomizer.CharacterLimiter;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    public sealed class PerformanceConfig : ConfigCategory
    {
        public override ModCompatibilityFlags CompatibilityFlags => base.CompatibilityFlags | ModCompatibilityFlags.RiskOfOptions;

        public readonly EnumConfigValue<CharacterLimitMode> SpawnLimitMode;

        public PerformanceConfig(ConfigFile file) : base("Performance", file)
        {
            SpawnLimitMode = new EnumConfigValue<CharacterLimitMode>(getEntry("Spawn Limit Mode", $"Limits the ability for characters to infinitely duplicate themselves.\n\n{nameof(CharacterLimitMode.Off)}: No limit is applied, characters can duplicate infinitely.\n\n{nameof(CharacterLimitMode.DecreaseByOneForEveryGeneration)}: Each summon has 1 less slot for summons than it's owner (A player spawned Goobo will have 2 Goobo slots, it's Goobo's will have 1 slot, and those Goobo's spawns will have no slots)\n\n{nameof(CharacterLimitMode.HalveForEveryGeneration)}: Each minion will have half as many spawns slots as it's owner.\n\n{nameof(CharacterLimitMode.DisableMinionSummon)}: Characters considered minions cannot spawn themselves at all. (Limits everything to 1 \"generation\")", CharacterLimitMode.DecreaseByOneForEveryGeneration));
        }
    }
}
