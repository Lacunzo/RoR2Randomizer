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
        public readonly BoolConfigValue RandomizeHitscanAttacks;

        public ProjectileRandomizerConfig(ConfigFile file) : base("Projectile", file)
        {
            RandomizeHitscanAttacks = new BoolConfigValue(getEntry("Randomize Hitscan Attacks", "If Hitscan attacks can be randomized into projectiles, and vice verse.", true));
        }
    }
}
