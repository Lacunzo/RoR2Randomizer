using RoR2;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.BossRandomizer.Aurelionite
{
    [PatchClass]
    static class OverrideTPEventSpawnPatch
    {
        static void Apply()
        {
            On.RoR2.GoldTitanManager.TryStartChannelingTitansServer += GoldTitanManager_TryStartChannelingTitansServer;
        }

        static void Cleanup()
        {
            On.RoR2.GoldTitanManager.TryStartChannelingTitansServer -= GoldTitanManager_TryStartChannelingTitansServer;
        }

        static bool GoldTitanManager_TryStartChannelingTitansServer(On.RoR2.GoldTitanManager.orig_TryStartChannelingTitansServer orig, object channeler, Vector3 approximatePosition, Vector3? lookAtPosition, Action channelEndCallback)
        {
            GameObject originalPrefab = null;

            bool overriddenPrefab;
            if (overriddenPrefab = BossRandomizerController.Aurelionite.TryGetAurelioniteMasterReplacementPrefab(out GameObject replacementPrefab))
            {
                originalPrefab = GoldTitanManager.goldTitanSpawnCard.prefab;
                GoldTitanManager.goldTitanSpawnCard.prefab = replacementPrefab;
            }

            bool result = orig(channeler, approximatePosition, lookAtPosition, channelEndCallback);

            if (overriddenPrefab)
            {
                GoldTitanManager.goldTitanSpawnCard.prefab = originalPrefab;
            }

            return result;
        }
    }
}
