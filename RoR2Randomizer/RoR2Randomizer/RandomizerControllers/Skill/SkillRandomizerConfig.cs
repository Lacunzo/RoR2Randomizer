#if !DISABLE_SKILL_RANDOMIZER
using BepInEx.Configuration;
using RoR2Randomizer.Configuration;

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