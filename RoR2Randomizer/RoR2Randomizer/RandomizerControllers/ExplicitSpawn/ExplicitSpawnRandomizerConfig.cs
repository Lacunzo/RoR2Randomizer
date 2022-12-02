using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    public sealed class ExplicitSpawnRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue RandomizeDirectorSpawns;

        public readonly BoolConfigValue RandomizeAbandonedAqueductRingEvent;

        public readonly BoolConfigValue RandomizeBeetleQueenSummonGuards;

        public readonly BoolConfigValue RandomizeVoidSeedMonsters;

        public readonly BoolConfigValue RandomizeRoboBallBuddies;

        public readonly BoolConfigValue RandomizeHeretic;

        public readonly BoolConfigValue RandomizeEngiTurrets;

        public readonly BoolConfigValue RandomizeMalachiteUrchins;

        public readonly BoolConfigValue RandomizeSoulWisps;

        public readonly BoolConfigValue RandomizeHealingCores;

        public readonly BoolConfigValue RandomizeVoidInfestors;

        public readonly BoolConfigValue RandomizeDefenseNucleusAlphaConstruct;

        public readonly BoolConfigValue RandomizeQueensGlandBeetleGuards;

        public readonly BoolConfigValue RandomizeRoboBallBossMinions;

        public readonly BoolConfigValue RandomizeShopkeeperNewt;

        public readonly BoolConfigValue RandomizeSquidTurrets;

        public readonly BoolConfigValue RandomizeDrones;

        public readonly BoolConfigValue RandomizeZoeaVoidAllies;

        public ExplicitSpawnRandomizerConfig(ConfigFile file) : base("Summon", file)
        {
            // RandomizeAbandonedAqueductRingEvent = new BoolConfigValue(getEntry("Randomize Runald & Kjaro", "Randomizes the character types of Runald and Kjaro on Abandoned Aqueduct.", true));

            RandomizeBeetleQueenSummonGuards = new BoolConfigValue(getEntry("Randomize Beetle Queen Guards", "Randomizes the character types of Beetle Guards spawned by Beetle Queens", true));

            RandomizeVoidSeedMonsters = new BoolConfigValue(getEntry("Randomize Void Seed enemies", "Randomizes the character types of Void enemies spawned in Void Seeds (Barnacles, Reavers, and Jailers).\n\nThis does not affect the voidtouched elites that are also spawned by the void seed.", true));

            RandomizeRoboBallBuddies = new BoolConfigValue(getEntry("Randomize Quiet & Delighted Probes", "Randomizes the character types of Quiet and Delighted Probes.", true));

            RandomizeHeretic = new BoolConfigValue(getEntry("Randomize Heretic", "Randomizes the character type you become when getting all the Heresy items (You still get the Heretic skills no matter which character type you become).", true));

            RandomizeEngiTurrets = new BoolConfigValue(getEntry("Randomize Engineer Turrets", "Randomizes the character type of Engineer Turrets.", true));

            RandomizeMalachiteUrchins = new BoolConfigValue(getEntry("Randomize Malachite Urchins", "Randomizes the character type of Malachite Urchins", true));

            RandomizeSoulWisps = new BoolConfigValue(getEntry("Randomize Soul Wisps", "Randomizes the character types of soul wisps", true));

            RandomizeHealingCores = new BoolConfigValue(getEntry("Randomize Healing Cores", "Randomizes the character type of Healing Cores", true));

            RandomizeVoidInfestors = new BoolConfigValue(getEntry("Randomize Void Infestors", "Randomizes the character type of Void Infestors", true));

            RandomizeDefenseNucleusAlphaConstruct = new BoolConfigValue(getEntry("Randomize Defense Nucleus Constructs", "Randomizes the character type of Alpha Constructs spawned by Defense Nucleus", true));

            RandomizeQueensGlandBeetleGuards = new BoolConfigValue(getEntry("Randomize Queens Gland Beetle Guards", "Randomizes the character type of Beetle Guards spawned by Queens Gland", true));

            RandomizeRoboBallBossMinions = new BoolConfigValue(getEntry("Randomize Solus Control Unit Probes", "Randomizes the character types of Solus Probes spawned by Solus Control Unit and Alloy Worship Unit", true));

            RandomizeShopkeeperNewt = new BoolConfigValue(getEntry("Randomize Shopkeeper Newt", "Randomizes the character type of the Shopkeeper Newt", true));

            RandomizeSquidTurrets = new BoolConfigValue(getEntry("Randomize Squid Turrets", "Randomizes the character type of Squid Turrets", true));

            RandomizeDrones = new BoolConfigValue(getEntry("Randomize Drones", "Randomizes the character types of drones and turrets", true));

            RandomizeZoeaVoidAllies = new BoolConfigValue(getEntry("Randomize Newly Hatched Zoea", "Randomizes the character types of the allies spawned by Newly Hatched Zoea", true));

            RandomizeDirectorSpawns = new BoolConfigValue(getEntry("Randomize Director Spawns", "Randomizes stage director spawns, no measures have been taken to \"balance\" the spawns, and anything can spawn. Basically Artifact of Dissonance on crack.\n\nDisabled by default.", false));
        }
    }
}
