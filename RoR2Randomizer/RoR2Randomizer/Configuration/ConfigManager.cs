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
        static readonly List<ConfigCategory> _allCategories = new List<ConfigCategory>();

        public static BossRandomizerConfig BossRandomizer;
        public static SkillRandomizerConfig SkillRandomizer;
        public static StageRandomizerConfig StageRandomizer;

        public static void Initialize(ConfigFile file)
        {
            _allCategories.Add(StageRandomizer = new StageRandomizerConfig(file));
            _allCategories.Add(SkillRandomizer = new SkillRandomizerConfig(file));
            _allCategories.Add(BossRandomizer = new BossRandomizerConfig(file));

            foreach (ConfigCategory category in _allCategories)
            {
                category.RunModCompatibilities();
            }
        }
    }
}
