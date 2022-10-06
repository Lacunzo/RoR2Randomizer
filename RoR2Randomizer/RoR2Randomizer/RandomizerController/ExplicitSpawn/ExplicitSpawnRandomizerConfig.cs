using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.ExplicitSpawn
{
    public sealed class ExplicitSpawnRandomizerConfig : BaseRandomizerConfig
    {
        public ExplicitSpawnRandomizerConfig(ConfigFile file) : base("Summon", file)
        {
        }
    }
}
