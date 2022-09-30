using HarmonyLib;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerController.Boss;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    [PatchClass]
    public static class GenericScriptedSpawnHook
    {
        public delegate bool OverrideSpawnPrefabDelegate(SpawnCard card, out GameObject overridePrefab);

        static OverrideSpawnPrefabDelegate _overrideSpawnPrefabFunc;
        public static OverrideSpawnPrefabDelegate OverrideSpawnPrefabFunc
        {
            get
            {
                return _overrideSpawnPrefabFunc;
            }
            set
            {
                if (_overrideSpawnPrefabFunc != null && value != null)
                    Log.Warning($"Assigning {nameof(GenericScriptedSpawnHook)}.{nameof(OverrideSpawnPrefabFunc)} while there is an existing hook, existing will be removed");

                _overrideSpawnPrefabFunc = value;
            }
        }

        public static event Action<SpawnCard.SpawnResult> OnSpawned;

        static void Apply()
        {
            On.RoR2.ScriptedCombatEncounter.Spawn += ScriptedCombatEncounter_Spawn;
        }

        static void Cleanup()
        {
            On.RoR2.ScriptedCombatEncounter.Spawn -= ScriptedCombatEncounter_Spawn;
        }

        static void ScriptedCombatEncounter_Spawn(On.RoR2.ScriptedCombatEncounter.orig_Spawn orig, ScriptedCombatEncounter self, ref ScriptedCombatEncounter.SpawnInfo spawnInfo)
        {
            GameObject originalPrefab = null;
            if (NetworkServer.active && _overrideSpawnPrefabFunc != null && _overrideSpawnPrefabFunc(spawnInfo.spawnCard, out GameObject overridePrefab))
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

            static void Postfix(ref SpawnCard.SpawnResult spawnResult)
            {
                if (spawnResult.success && spawnResult.spawnedInstance && spawnResult.spawnRequest != null && spawnResult.spawnRequest.spawnCard)
                {
                    OnSpawned?.Invoke(spawnResult);
                }
            }
        }
    }
}
