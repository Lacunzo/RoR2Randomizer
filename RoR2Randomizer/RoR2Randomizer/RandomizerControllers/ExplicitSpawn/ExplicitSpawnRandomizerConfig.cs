using BepInEx.Configuration;
using RoR2Randomizer.Configuration;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    public sealed class ExplicitSpawnRandomizerConfig : BaseRandomizerConfig
    {
        public ExplicitSpawnRandomizerConfig(ConfigFile file) : base("Summon", file)
        {
        }
    }
}
