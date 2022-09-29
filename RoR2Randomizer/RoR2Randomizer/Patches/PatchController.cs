using HarmonyLib;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.RandomizerController.Buff;
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

            BossRandomizer.MainPatcher.Apply();
            SkillPatcher.Apply();

            MultiEntityStatePatches.MainPatcher.Apply();

            BuffRandomizer.MainPatcher.Apply();

            SurvivorPodRandomizer.OverrideIntroAnimation.Apply();

            _patchControllersRoot = new GameObject(Main.PluginName + ".PatchControllers");
            GameObject.DontDestroyOnLoad(_patchControllersRoot);

#if DEBUG
            _patchControllersRoot.AddComponent<DebugButtonsManager>();
            _patchControllersRoot.AddComponent<Debug.SpawnDisabler>();
#endif

            _patchControllersRoot.AddComponent<BossRandomizerController>();
#if !DISABLE_SKILL_RANDOMIZER
            _patchControllersRoot.AddComponent<SkillRandomizerController>();
#endif
            _patchControllersRoot.AddComponent<StageRandomizerController>();
            _patchControllersRoot.AddComponent<BuffRandomizerController>();
            _patchControllersRoot.AddComponent<SurvivorPodRandomizerController>();
        }

        public static void Cleanup()
        {
            GameObject.Destroy(_patchControllersRoot);
            _patchControllersRoot = null;

            _harmonyInstance.UnpatchSelf();

            BossRandomizer.MainPatcher.Cleanup();
            SkillPatcher.Cleanup();

            MultiEntityStatePatches.MainPatcher.Cleanup();

            BuffRandomizer.MainPatcher.Cleanup();

            SurvivorPodRandomizer.OverrideIntroAnimation.Cleanup();
        }
    }
}
