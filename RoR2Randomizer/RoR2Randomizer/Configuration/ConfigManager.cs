using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.RandomizerController.Skill;
using RoR2Randomizer.RandomizerController.Stage;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    public static class ConfigManager
    {
        static readonly List<ConfigCategory> _riskOfOptionsCategories = new List<ConfigCategory>();

        public static BossRandomizerConfig BossRandomizer;
        public static SkillRandomizerConfig SkillRandomizer;
        public static StageRandomizerConfig StageRandomizer;

        public static void Initialize(ConfigFile file)
        {
            _riskOfOptionsCategories.Add(StageRandomizer = new StageRandomizerConfig(file));
            _riskOfOptionsCategories.Add(SkillRandomizer = new SkillRandomizerConfig(file));
            _riskOfOptionsCategories.Add(BossRandomizer = new BossRandomizerConfig(file));

            if (ModCompatibility.RiskOfOptionsCompat.IsEnabled)
            {
                foreach (ConfigCategory category in _riskOfOptionsCategories)
                {
                    category.RiskOfOptionsCompatibility();
                }
            }
        }
    }
}
