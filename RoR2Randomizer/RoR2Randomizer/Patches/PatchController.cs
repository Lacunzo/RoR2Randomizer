using HarmonyLib;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.RandomizerController.Skill;
using RoR2Randomizer.RandomizerController.Stage;
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
            _patchControllersRoot = new GameObject(Main.PluginName + ".PatchControllers");
            GameObject.DontDestroyOnLoad(_patchControllersRoot);

#if DEBUG
            _patchControllersRoot.AddComponent<Debug.SpawnDisabler>();
#endif

            _patchControllersRoot.AddComponent<BossRandomizerController>();
            _patchControllersRoot.AddComponent<SkillRandomizerController>();
            _patchControllersRoot.AddComponent<StageRandomizerController>();

            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Fixes.EntityStateOwnerSkill.MainPatcher.Apply();

            BossRandomizer.MainPatcher.Apply();
        }

        public static void Cleanup()
        {
            GameObject.Destroy(_patchControllersRoot);
            _patchControllersRoot = null;

            _harmonyInstance.UnpatchSelf();

            Fixes.EntityStateOwnerSkill.MainPatcher.Cleanup();

            BossRandomizer.MainPatcher.Cleanup();
        }
    }
}
