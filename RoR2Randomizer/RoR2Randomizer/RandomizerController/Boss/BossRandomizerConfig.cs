using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public class BossRandomizerConfig : BaseRandomizerConfig
    {
        public readonly BoolConfigValue RandomizeMithrix;
        public readonly BoolConfigValue RandomizeMithrixPhase2;
        public bool AnyMithrixRandomizerEnabled => Enabled && (RandomizeMithrix || RandomizeMithrixPhase2);

        public readonly BoolConfigValue RandomizeVoidling;

        public readonly BoolConfigValue RandomizeAurelionite;

#if DEBUG
        public readonly EnumConfigValue<BossRandomizerController.DebugMode> BossDebugMode;
        public readonly StringConfigValue DebugBossForcedMasterName;
#endif

        public BossRandomizerConfig(ConfigFile file) : base("Boss", file)
        {
            RandomizeMithrix = new BoolConfigValue(getEntry("Randomize Mithrix", "Randomizes the character type of Mithrix", true));
            RandomizeMithrixPhase2 = new BoolConfigValue(getEntry("Randomize Mithrix Phase 2", "Randomizes the character type of all the characters spawned during Mithrix phase 2", true));

            RandomizeVoidling = new BoolConfigValue(getEntry("Randomize Voidling", "Randomizes the character type of Voidling", true));

            RandomizeAurelionite = new BoolConfigValue(getEntry("Randomize Aurelionite", "Randomizes the character type of Aurelionite", true));

#if DEBUG
            BossDebugMode = new EnumConfigValue<BossRandomizerController.DebugMode>(getEntry("Boss Debug Mode", BossRandomizerController.DebugMode.None));
            DebugBossForcedMasterName = new StringConfigValue(getEntry("Boss Debug Forced Master Name", "EquipmentDroneMaster"));
#endif
        }
    }
}
