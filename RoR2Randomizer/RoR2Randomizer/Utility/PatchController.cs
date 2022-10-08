using HarmonyLib;
using RoR2Randomizer.Patches;
using RoR2Randomizer.RandomizerControllers;
using System;
using System.Reflection;
using UnityEngine;

namespace RoR2Randomizer.Utility
{
    public static class InitializationManager
    {
        static Harmony _harmonyInstance;

        static GameObject _controllersRoot;

        public static void Init()
        {
            _harmonyInstance = new Harmony(Main.PluginGUID);
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            PatchClassAttribute.ApplyAllPatches();

            _controllersRoot = new GameObject(Main.PluginName + ".Controllers");
            UnityEngine.Object.DontDestroyOnLoad(_controllersRoot);

            foreach (Type controllerType in RandomizerControllerAttribute.RandomizerControllerTypes.Get)
            {
                if (controllerType != null)
                {
                    _controllersRoot.AddComponent(controllerType);
                }
            }
        }

        public static void Cleanup()
        {
            UnityEngine.Object.Destroy(_controllersRoot);
            _controllersRoot = null;

            _harmonyInstance.UnpatchSelf();
            _harmonyInstance = null;

            PatchClassAttribute.CleanupAllPatches();
        }
    }
}
