using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.LunarScav;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    public partial class BossRandomizerController
    {
        public static class LunarScav
        {
            static bool IsEnabled => _instance && _instance.IsRandomizerEnabled && ConfigManager.BossRandomizer.RandomizeLunarScav;

            public static event Action<LunarScavReplacement> LunarScavReplacementReceivedClient;

            public static void Initialize()
            {
                if (LunarScavFightTracker.Instance != null)
                {
                    LunarScavFightTracker.Instance.IsInFight.OnChanged += IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;
            }

            public static void Uninitialize()
            {
                if (LunarScavFightTracker.Instance != null)
                {
                    LunarScavFightTracker.Instance.IsInFight.OnChanged -= IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;
            }

            static void IsInFight_OnChanged(bool isInFight)
            {
                if (IsEnabled)
                {
                    if (isInFight)
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = (ref SpawnCard card, out GenericScriptedSpawnHook.ResetCardDelegate resetCardFunc) =>
                        {
                            resetCardFunc = null;

                            if (SpawnCardTracker.LunarScavSpawnCard == card)
                            {
                                if (card is MultiCharacterSpawnCard multiCard)
                                {
                                    GameObject[] originalMasterPrefabs = new GameObject[multiCard.masterPrefabs.Length];

                                    resetCardFunc = (ref SpawnCard c) =>
                                    {
                                        MultiCharacterSpawnCard multiSpawn = (MultiCharacterSpawnCard)c;

                                        if (originalMasterPrefabs.Length != multiSpawn.masterPrefabs.Length)
                                        {
                                            Log.Warning($"LunarScavRandomizer: original master prefabs and card prefabs have different sizes");
                                            return;
                                        }

                                        Array.Copy(originalMasterPrefabs, multiSpawn.masterPrefabs, originalMasterPrefabs.Length);
                                    };

                                    for (int i = 0; i < multiCard.masterPrefabs.Length; i++)
                                    {
                                        originalMasterPrefabs[i] = multiCard.masterPrefabs[i];
                                        CharacterReplacements.TryReplaceMasterPrefab(ref multiCard.masterPrefabs[i]);
                                    }
                                }
                                else
                                {
                                    Log.Warning($"LunarScavRandomizer: {nameof(SpawnCard)} is not a {nameof(MultiCharacterSpawnCard)}");
                                }
                            }
                        };

                        GenericScriptedSpawnHook.OnSpawned += handleSpawnedLunarScavCharacterServer;
                    }
                    else
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = null;

                        GenericScriptedSpawnHook.OnSpawned -= handleSpawnedLunarScavCharacterServer;
                    }
                }
            }

            static void SyncBossReplacementCharacter_OnReceive(GameObject masterObject, BossReplacementType replacementType)
            {
                switch (replacementType)
                {
                    case BossReplacementType.LunarScav1:
                    case BossReplacementType.LunarScav2:
                    case BossReplacementType.LunarScav3:
                    case BossReplacementType.LunarScav4:
                        handleSpawnedLunarScavCharacterClient(masterObject, replacementType - BossReplacementType.LunarScav1);
                        break;
                }
            }

            static void handleSpawnedLunarScavCharacterClient(GameObject masterObject, uint scavIndex)
            {
                LunarScavReplacement lunarScavReplacement = masterObject.AddComponent<LunarScavReplacement>();
#if DEBUG
                Log.Debug($"Adding {nameof(LunarScavReplacement)} component to {masterObject} ScavIndex={scavIndex}");
#endif
                lunarScavReplacement.LunarScavIndex = scavIndex;
                lunarScavReplacement.Initialize();

                LunarScavReplacementReceivedClient?.Invoke(lunarScavReplacement);
            }

            static void handleSpawnedLunarScavCharacterServer(SpawnCard.SpawnResult spawnResult)
            {
                if (IsEnabled &&
                    LunarScavFightTracker.Instance != null && LunarScavFightTracker.Instance.IsInFight &&
                    SpawnCardTracker.LunarScavSpawnCard == spawnResult.spawnRequest.spawnCard)
                {
                    LunarScavReplacement lunarScavReplacement = spawnResult.spawnedInstance.AddComponent<LunarScavReplacement>();

                    int index = Array.IndexOf(SpawnCardTracker.LunarScavSpawnCard.masterPrefabs, spawnResult.spawnRequest.spawnCard.prefab);
                    if (index != -1)
                        lunarScavReplacement.LunarScavIndex = (uint)index;
#if DEBUG
                    Log.Debug($"Adding {nameof(LunarScavReplacement)} component to {spawnResult.spawnedInstance} ScavIndex={lunarScavReplacement.LunarScavIndex}");
#endif
                    lunarScavReplacement.Initialize();
                }
            }
        }
    }
}
