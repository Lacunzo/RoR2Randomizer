using HarmonyLib;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.RandomizerController.Buff;
using RoR2Randomizer.RandomizerController.Effect;
using RoR2Randomizer.RandomizerController.ExplicitSpawn;
using RoR2Randomizer.RandomizerController.Projectile;
#if !DISABLE_SKILL_RANDOMIZER
using RoR2Randomizer.RandomizerController.Skill;
#endif
using RoR2Randomizer.RandomizerController.Stage;
using RoR2Randomizer.RandomizerController.SurvivorPod;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches
{
    public static class PatchController
    {
        static readonly Harmony _harmonyInstance = new Harmony(Main.PluginGUID);

        static GameObject _patchControllersRoot;

        public static void Setup()
        {
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            PatchClassAttribute.ApplyAllPatches();

            _patchControllersRoot = new GameObject(Main.PluginName + ".PatchControllers");
            GameObject.DontDestroyOnLoad(_patchControllersRoot);

#if DEBUG
            _patchControllersRoot.AddComponent<DebugButtonsManager>();
#endif

            _patchControllersRoot.AddComponent<BossRandomizerController>();
#if !DISABLE_SKILL_RANDOMIZER
            _patchControllersRoot.AddComponent<SkillRandomizerController>();
#endif
            _patchControllersRoot.AddComponent<StageRandomizerController>();
            _patchControllersRoot.AddComponent<BuffRandomizerController>();
            _patchControllersRoot.AddComponent<SurvivorPodRandomizerController>();
            _patchControllersRoot.AddComponent<ProjectileRandomizerController>();
            _patchControllersRoot.AddComponent<ExplicitSpawnRandomizerController>();
            _patchControllersRoot.AddComponent<EffectRandomizerController>();
        }

        public static void Cleanup()
        {
            GameObject.Destroy(_patchControllersRoot);
            _patchControllersRoot = null;

            _harmonyInstance.UnpatchSelf();

            PatchClassAttribute.CleanupAllPatches();
        }
    }
}
