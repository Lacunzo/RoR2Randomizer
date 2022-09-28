using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Buff
{
    public sealed class BuffRandomizerConfig : BaseRandomizerConfig
    {
        public BoolConfigValue MixBuffsAndDebuffs;

        public BuffRandomizerConfig(ConfigFile file) : base("Status Effect", file)
        {
            MixBuffsAndDebuffs = new BoolConfigValue(getEntry("Mix buffs and debuffs", "If the randomizer should be able to turn a buff (positive effect) into a debuff (negative effect) and vice versa.", true));
        }
    }
}
