using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    public class MiscConfig : ConfigCategory
    {
        public override ModCompatibilityFlags CompatibilityFlags => base.CompatibilityFlags | ModCompatibilityFlags.RiskOfOptions;

        public readonly BoolConfigValue SurvivorPodRandomizerEnabled;

        public readonly BoolConfigValue EffectRandomizerEnabled;

        public MiscConfig(ConfigFile file) : base("Miscellaneous", file)
        {
            SurvivorPodRandomizerEnabled = new BoolConfigValue(getEntry("Survivor Spawn Pod Randomizer", "Randomizes the intro animation of all survivors.", true));

            EffectRandomizerEnabled = new BoolConfigValue(getEntry("Effect Randomizer", "Randomizes various visual effects. (Epilepsy Warning)", false));
        }
    }
}
