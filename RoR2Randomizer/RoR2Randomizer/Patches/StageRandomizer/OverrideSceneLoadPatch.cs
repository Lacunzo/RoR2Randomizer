using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2Randomizer.RandomizerController.Stage;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageRandomizer
{
    public static class OverrideSceneLoadPatch
    {
        static readonly Hook UnityEngine_Networking_NetworkManager_ServerChangeScene_Hook = new Hook(SymbolExtensions.GetMethodInfo<NetworkManager>(_ => _.ServerChangeScene(default)), (Action<NetworkManager, string> orig, NetworkManager self, string newSceneName) =>
        {
            if (StageRandomizerController.TryGetReplacementSceneName(newSceneName, out string replacementSceneName))
                newSceneName = replacementSceneName;

            orig(self, newSceneName);
        });

        public static void Apply()
        {
            UnityEngine_Networking_NetworkManager_ServerChangeScene_Hook?.Apply();
        }

        public static void Cleanup()
        {
            UnityEngine_Networking_NetworkManager_ServerChangeScene_Hook?.Undo();
        }
    }
}
