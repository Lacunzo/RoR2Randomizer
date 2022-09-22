using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController.Boss;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    public static class SpawnHook
    {
        public static void Apply()
        {
            On.RoR2.ScriptedCombatEncounter.Spawn += ScriptedCombatEncounter_Spawn;
        }

        public static void Cleanup()
        {
            On.RoR2.ScriptedCombatEncounter.Spawn -= ScriptedCombatEncounter_Spawn;
        }

        static void ScriptedCombatEncounter_Spawn(On.RoR2.ScriptedCombatEncounter.orig_Spawn orig, ScriptedCombatEncounter self, ref ScriptedCombatEncounter.SpawnInfo spawnInfo)
        {
            GameObject originalPrefab = null;
            if (NetworkServer.active && ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled && BossRandomizerController.Mithrix.TryGetOverridePrefabFor(spawnInfo.spawnCard, out GameObject overridePrefab))
            {
                originalPrefab = spawnInfo.spawnCard.prefab;
                spawnInfo.spawnCard.prefab = overridePrefab;
            }

            orig(self, ref spawnInfo);

            if (originalPrefab)
            {
                spawnInfo.spawnCard.prefab = originalPrefab;
            }
        }

        [HarmonyPatch]
        static class ScriptedCombatEncounter_Spawn_HandleSpawn_Patch
        {
            static MethodBase TargetMethod()
            {
                return typeof(ScriptedCombatEncounter).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).SingleOrDefault(m => m.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null && m.Name.StartsWith("<Spawn>g__HandleSpawn|"));
            }

            static void Prefix(ref SpawnCard.SpawnResult spawnResult)
            {
                if (spawnResult.success && spawnResult.spawnedInstance && spawnResult.spawnRequest != null && spawnResult.spawnRequest.spawnCard)
                {
                    BossRandomizerController.Mithrix.HandleSpawnedMithrixCharacterServer(spawnResult);
                }
            }
        }
    }
}
