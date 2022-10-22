#if DEBUG
using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using RoR2Randomizer.RandomizerControllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    public sealed class DebugConfig : ConfigCategory
    {
        public override ModCompatibilityFlags CompatibilityFlags => base.CompatibilityFlags | ModCompatibilityFlags.RiskOfOptions;

        public readonly EnumConfigValue<DebugMode> CharacterDebugMode;
        public readonly StringConfigValue ForcedMasterName;

        public readonly BoolConfigValue AllowLocalhostConnect;

        public DebugConfig(ConfigFile file) : base("Debug", file)
        {
            CharacterDebugMode = new EnumConfigValue<DebugMode>(getEntry("Character Debug Mode", DebugMode.None));
            ForcedMasterName = new StringConfigValue(getEntry("Forced Master Name", string.Empty));

            AllowLocalhostConnect = new BoolConfigValue(getEntry("Allow Localhost Connect", false));
        }
    }
}
#endif