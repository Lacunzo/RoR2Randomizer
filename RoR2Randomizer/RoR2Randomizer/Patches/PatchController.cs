using HarmonyLib;
using RoR2Randomizer.RandomizerController;
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

            _patchControllersRoot.AddComponent<SkillRandomizerController>();

            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Fixes.EntityStateOwnerSkill.MainPatch.Apply();

            CharacterRandomizer.MainPatcher.Apply();
        }

        public static void Cleanup()
        {
            GameObject.Destroy(_patchControllersRoot);
            _patchControllersRoot = null;

            _harmonyInstance.UnpatchSelf();

            Fixes.EntityStateOwnerSkill.MainPatch.Cleanup();

            CharacterRandomizer.MainPatcher.Cleanup();
        }
    }
}
