using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.RandomizerController.Buff;
using RoR2Randomizer.RandomizerController.ExplicitSpawn;
using RoR2Randomizer.RandomizerController.Projectile;
#if !DISABLE_SKILL_RANDOMIZER
using RoR2Randomizer.RandomizerController.Skill;
#endif
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

        public static BuffRandomizerConfig BuffRandomizer;
        public static BossRandomizerConfig BossRandomizer;
#if !DISABLE_SKILL_RANDOMIZER
        public static SkillRandomizerConfig SkillRandomizer;
#endif
        public static StageRandomizerConfig StageRandomizer;
        public static ProjectileRandomizerConfig ProjectileRandomizer;
        public static ExplicitSpawnRandomizerConfig ExplicitSpawnRandomizer;
        public static MiscConfig Misc;

#if DEBUG
        public static DebugConfig Debug;
#endif

        public static void Initialize(ConfigFile file)
        {
            _allCategories.Add(BuffRandomizer = new BuffRandomizerConfig(file));
            _allCategories.Add(StageRandomizer = new StageRandomizerConfig(file));
#if !DISABLE_SKILL_RANDOMIZER
            _allCategories.Add(SkillRandomizer = new SkillRandomizerConfig(file));
#endif
            _allCategories.Add(BossRandomizer = new BossRandomizerConfig(file));
            _allCategories.Add(ProjectileRandomizer = new ProjectileRandomizerConfig(file));
            _allCategories.Add(ExplicitSpawnRandomizer = new ExplicitSpawnRandomizerConfig(file));

            _allCategories.Sort((a, b) => a.CategoryName.CompareTo(b.CategoryName));

            _allCategories.Add(Misc = new MiscConfig(file));
#if DEBUG
            _allCategories.Add(Debug = new DebugConfig(file));
#endif

            foreach (ConfigCategory category in _allCategories)
            {
                category.RunModCompatibilities();
            }
        }
    }
}
