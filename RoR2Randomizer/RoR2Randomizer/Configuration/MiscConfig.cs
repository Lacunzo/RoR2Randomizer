using BepInEx.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;

namespace RoR2Randomizer.Configuration
{
    public class MiscConfig : ConfigCategory
    {
        public override ModCompatibilityFlags CompatibilityFlags => base.CompatibilityFlags | ModCompatibilityFlags.RiskOfOptions;

        public readonly BoolConfigValue SurvivorPodRandomizerEnabled;

        public readonly BoolConfigValue EffectRandomizerEnabled;

        public readonly BoolConfigValue SniperWeakPointRandomizerEnabled;

        public MiscConfig(ConfigFile file) : base("Miscellaneous", file)
        {
            SurvivorPodRandomizerEnabled = new BoolConfigValue(getEntry("Survivor Spawn Pod Randomizer", "Randomizes the intro animation of all survivors.", true));

            EffectRandomizerEnabled = new BoolConfigValue(getEntry("Effect Randomizer", "Randomizes various visual effects. (Potential Epilepsy Warning)", false));

            SniperWeakPointRandomizerEnabled = new BoolConfigValue(getEntry("Weak Point Randomizer", "Randomizes which hitboxes are considered weak points for railgunner's scoped shot. The number of weak points on a character does not change.", true));
        }
    }
}
