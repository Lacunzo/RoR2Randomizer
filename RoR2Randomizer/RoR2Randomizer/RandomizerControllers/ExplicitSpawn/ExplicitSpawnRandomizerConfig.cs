using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    public sealed class ExplicitSpawnRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue RandomizeDirectorSpawns;

        public ExplicitSpawnRandomizerConfig(ConfigFile file) : base("Summon", file)
        {
            RandomizeDirectorSpawns = new BoolConfigValue(getEntry("Randomize Director Spawns", "Randomizes stage director spawns, no measures have been taken to \"balance\" the spawns, and literally anything can spawn.\n\nDisabled by default", false));
        }
    }
}
