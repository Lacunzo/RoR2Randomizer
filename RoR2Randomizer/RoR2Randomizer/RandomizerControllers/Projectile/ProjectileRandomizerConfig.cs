using BepInEx.Configuration;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.RandomizerControllers.Boss;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerControllers.Projectile
{
    public sealed class ProjectileRandomizerConfig : BaseRandomizerConfig
    {
#if DEBUG
        public readonly EnumConfigValue<DebugMode> DebugMode;
        public readonly IntStringConfigValue ForcedProjectileIndex;
#endif

        public ProjectileRandomizerConfig(ConfigFile file) : base("Projectile", file)
        {
#if DEBUG
            DebugMode = new EnumConfigValue<DebugMode>(getEntry("Debug Mode", RandomizerControllers.DebugMode.None));

            ForcedProjectileIndex = new IntStringConfigValue(getEntry("Forced Projectile Index", "0"), -1);
#endif
        }
    }
}
