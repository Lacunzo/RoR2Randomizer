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

#if DEBUG
        public readonly EnumConfigValue<BossRandomizerController.Mithrix.DebugMode> MithrixDebugMode;
        public readonly StringConfigValue DebugMithrixForcedMasterName;
#endif

        public bool AnyMithrixRandomizerEnabled => Enabled && (RandomizeMithrix || RandomizeMithrixPhase2);

        public BossRandomizerConfig(ConfigFile file) : base("Boss Randomizer", file)
        {
            RandomizeMithrix = new BoolConfigValue(getEntry("Randomize Mithrix", "Randomizes the character type of Mithrix", true));
            RandomizeMithrixPhase2 = new BoolConfigValue(getEntry("Randomize Mithrix Phase 2", "Randomizes the character type of all the characters spawned during Mithrix phase 2", true));

#if DEBUG
            MithrixDebugMode = new EnumConfigValue<BossRandomizerController.Mithrix.DebugMode>(getEntry("Mithrix Debug Mode", BossRandomizerController.Mithrix.DebugMode.None));
            DebugMithrixForcedMasterName = new StringConfigValue(getEntry("Mithrix Debug Forced Master Name", "EquipmentDroneMaster"));
#endif
        }
    }
}
