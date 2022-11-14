using HarmonyLib;
using MonoMod.RuntimeDetour;
using RoR2Randomizer.RandomizerControllers.Stage;
using System;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageRandomizer
{
    [PatchClass]
    public static class OverrideSceneLoadPatch
    {
        static readonly Hook UnityEngine_Networking_NetworkManager_ServerChangeScene_Hook = new Hook(SymbolExtensions.GetMethodInfo<NetworkManager>(_ => _.ServerChangeScene(default)), (Action<NetworkManager, string> orig, NetworkManager self, string newSceneName) =>
        {
            if (StageRandomizerController.TryGetReplacementSceneName(newSceneName, out string replacementSceneName))
                newSceneName = replacementSceneName;

            orig(self, newSceneName);
        });

        static void Apply()
        {
            UnityEngine_Networking_NetworkManager_ServerChangeScene_Hook?.Apply();
        }

        static void Cleanup()
        {
            UnityEngine_Networking_NetworkManager_ServerChangeScene_Hook?.Undo();
        }
    }
}
