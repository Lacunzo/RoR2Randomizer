using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.AlloyWorshipUnit;
using RoR2Randomizer.Patches.BossRandomizer.Aurelionite;
using RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public partial class BossRandomizerController : Singleton<BossRandomizerController>
    {
        public static class AlloyWorshipUnit
        {
            public static void Initialize()
            {
                if (AlloyWorshipUnitFightTracker.Instance != null)
                {
                    AlloyWorshipUnitFightTracker.Instance.IsInFight.OnChanged += IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;
            }

            public static void Uninitialize()
            {
                if (AlloyWorshipUnitFightTracker.Instance != null)
                {
                    AlloyWorshipUnitFightTracker.Instance.IsInFight.OnChanged -= IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;
            }

            static void IsInFight_OnChanged(bool isInFight)
            {
                if (NetworkServer.active)
                {
                    if (isInFight)
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = (ref SpawnCard card, out GenericScriptedSpawnHook.ResetCardDelegate resetCardFunc) =>
                        {
                            if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeAlloyWorshipUnit && card == SpawnCardTracker.AlloyWorshipUnitSpawnCard)
                            {
                                GameObject originalPrefab = card.prefab;
                                resetCardFunc = (ref SpawnCard c) => c.prefab = originalPrefab;

                                CharacterReplacements.TryReplaceMasterPrefab(ref card.prefab);
                            }
                            else
                            {
                                resetCardFunc = null;
                            }
                        };

                        GenericScriptedSpawnHook.OnSpawned += handleSpawnedAWUCharacterServer;
                    }
                    else
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = null;
                        GenericScriptedSpawnHook.OnSpawned -= handleSpawnedAWUCharacterServer;
                    }
                }
            }

            static void SyncBossReplacementCharacter_OnReceive(GameObject masterObject, BossReplacementType replacementType)
            {
                switch (replacementType)
                {
                    case BossReplacementType.AlloyWorshipUnit:
                        handleSpawnedAWUCharacterClient(masterObject);
                        break;
                }
            }

            static void handleSpawnedAWUCharacterClient(GameObject masterObject)
            {
                AlloyWorshipUnitReplacement awuReplacement = masterObject.AddComponent<AlloyWorshipUnitReplacement>();
#if DEBUG
                Log.Debug($"Adding {nameof(AlloyWorshipUnitReplacement)} component to {masterObject}");
#endif
                awuReplacement.Initialize();
            }

            static void handleSpawnedAWUCharacterServer(SpawnCard.SpawnResult spawnResult)
            {
                if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeAlloyWorshipUnit &&
                    AlloyWorshipUnitFightTracker.Instance != null && AlloyWorshipUnitFightTracker.Instance.IsInFight &&
                    spawnResult.spawnRequest.spawnCard == SpawnCardTracker.AlloyWorshipUnitSpawnCard)
                {
                    AlloyWorshipUnitReplacement awuReplacement = spawnResult.spawnedInstance.AddComponent<AlloyWorshipUnitReplacement>();
#if DEBUG
                    Log.Debug($"Adding {nameof(AlloyWorshipUnitReplacement)} component to {spawnResult.spawnedInstance}");
#endif
                    awuReplacement.Initialize();
                }
            }
        }
    }
}
