using BepInEx.Configuration;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Projectile
{
    public sealed class ProjectileRandomizerConfig : BaseRandomizerConfig
    {
#if DEBUG
        public readonly EnumConfigValue<DebugMode> DebugMode;
        public readonly StringConfigValue ForcedProjectileIndex;
#endif

        public ProjectileRandomizerConfig(ConfigFile file) : base("Projectile", file)
        {
#if DEBUG
            DebugMode = new EnumConfigValue<DebugMode>(getEntry("Debug Mode", RandomizerController.DebugMode.None));

            ForcedProjectileIndex = new StringConfigValue(getEntry("Forced Projectile Index", "0"));
#endif
        }
    }
}
