using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Projectile
{
    public sealed class ProjectileRandomizerConfig : BaseRandomizerConfig
    {
        public ProjectileRandomizerConfig(ConfigFile file) : base("Projectile", file)
        {
        }
    }
}
