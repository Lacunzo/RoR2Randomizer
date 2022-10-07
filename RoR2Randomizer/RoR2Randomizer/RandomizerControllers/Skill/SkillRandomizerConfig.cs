#if !DISABLE_SKILL_RANDOMIZER
using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerController.Skill
{
    public class SkillRandomizerConfig : BaseRandomizerConfig
    {
        public SkillRandomizerConfig(ConfigFile file) : base("Skill Randomizer", file)
        {
        }
    }
}
#endif