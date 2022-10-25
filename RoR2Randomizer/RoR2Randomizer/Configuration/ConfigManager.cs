using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RoR2Randomizer.RandomizerControllers.Boss;
using RoR2Randomizer.RandomizerControllers.Buff;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
#endif
using RoR2Randomizer.RandomizerControllers.Projectile;
#if !DISABLE_SKILL_RANDOMIZER
using RoR2Randomizer.RandomizerController.Skill;
#endif
using RoR2Randomizer.RandomizerControllers.Stage;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Configuration
{
    public static class ConfigManager
    {
        static readonly List<ConfigCategory> _allCategories = new List<ConfigCategory>();

        internal static MetadataConfig Metadata;

        public static BuffRandomizerConfig BuffRandomizer;
        public static BossRandomizerConfig BossRandomizer;
#if !DISABLE_SKILL_RANDOMIZER
        public static SkillRandomizerConfig SkillRandomizer;
#endif
        public static StageRandomizerConfig StageRandomizer;
        public static ProjectileRandomizerConfig ProjectileRandomizer;
        public static ExplicitSpawnRandomizerConfig ExplicitSpawnRandomizer;
#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
        public static HoldoutZoneRandomizerConfig HoldoutZoneRandomizer;
#endif

        public static PerformanceConfig Performance;

        public static FunConfig Fun;

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
#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
            _allCategories.Add(HoldoutZoneRandomizer = new HoldoutZoneRandomizerConfig(file));
#endif

            _allCategories.Sort((a, b) => a.CategoryName.CompareTo(b.CategoryName));

            _allCategories.Add(Performance = new PerformanceConfig(file));

            _allCategories.Add(Fun = new FunConfig(file));

            _allCategories.Add(Misc = new MiscConfig(file));
#if DEBUG
            _allCategories.Add(Debug = new DebugConfig(file));
#endif

            _allCategories.Insert(0, Metadata = new MetadataConfig(file));

            foreach (ConfigCategory category in _allCategories)
            {
                category.RunModCompatibilities();
            }
        }
    }
}
