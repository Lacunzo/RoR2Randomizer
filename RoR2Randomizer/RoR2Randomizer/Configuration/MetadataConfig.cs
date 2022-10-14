using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    sealed class MetadataConfig : ConfigCategory
    {
        public readonly BoolConfigValue DisplayModInfoOnStart;

        public MetadataConfig(ConfigFile file) : base("Metadata (These settings are not intended for users to modify)", file)
        {
            DisplayModInfoOnStart = new BoolConfigValue(getEntry("SHOW_MOD_INFO_MESSAGE", true));
        }
    }
}
