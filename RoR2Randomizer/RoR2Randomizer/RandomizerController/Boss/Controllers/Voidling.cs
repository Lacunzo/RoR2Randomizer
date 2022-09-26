using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Voidling;
using RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using EntityStates;
using RoR2Randomizer.RandomizerController.Stage;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public partial class BossRandomizerController : Singleton<BossRandomizerController>
    {
        public static class Voidling
        {
            public static readonly SerializableEntityStateType EscapeDeathState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.EscapeDeath));
            public static readonly SerializableEntityStateType FinalDeathState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.DeathState));

            public static void Initialize()
            {
                if (VoidlingPhaseTracker.Instance != null)
                {
                    VoidlingPhaseTracker.Instance.OnEnterFight += onEnterVoidlingFight;
                    VoidlingPhaseTracker.Instance.OnExitFight += onExitVoidlingFight;
                }

                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;

                SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;
            }

            public static void Uninitialize()
            {
                if (VoidlingPhaseTracker.Instance != null)
                {
                    VoidlingPhaseTracker.Instance.OnEnterFight -= onEnterVoidlingFight;
                    VoidlingPhaseTracker.Instance.OnExitFight -= onExitVoidlingFight;
                }

                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;

                SceneCatalog.onMostRecentSceneDefChanged -= SceneCatalog_onMostRecentSceneDefChanged;
            }

            static void SceneCatalog_onMostRecentSceneDefChanged(SceneDef obj)
            {
                if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeVoidling && obj.cachedName == StageRandomizerController.VOIDLING_FIGHT_SCENE_NAME)
                {
                    GameObject levelRoot = GameObject.Find("RaidVoid");
                    if (levelRoot)
                    {
                        // Disable some blobs that often obscure the replaced voidling
                        Transform blob = levelRoot.transform.Find("RaidVoidProps/CrabFoam1Prop (14)");
                        if (blob)
                        {
                            blob.gameObject.SetActive(false);
                        }
                    }
                }
            }

            static void onEnterVoidlingFight()
            {
                if (NetworkServer.active)
                {
                    GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = (SpawnCard card, out GameObject overridePrefab) =>
                    {
                        if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeVoidling && SpawnCardTracker.IsAnyVoidlingPhase(card))
                        {
                            overridePrefab = getBossOverrideMasterPrefab();

#if DEBUG
                            Log.Debug($"VoidlingRandomizer: Replaced {card.prefab} with {overridePrefab}");
#endif

                            return (bool)overridePrefab;
                        }

                        overridePrefab = null;
                        return false;
                    };

                    GenericScriptedSpawnHook.OnSpawned += handleSpawnedVoidlingCharacterServer;
                }
            }

            static void onExitVoidlingFight()
            {
                if (NetworkServer.active)
                {
                    GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = null;
                    GenericScriptedSpawnHook.OnSpawned -= handleSpawnedVoidlingCharacterServer;
                }
            }

            static void SyncBossReplacementCharacter_OnReceive(GameObject masterObject, BossReplacementType replacementType)
            {
                if (VoidlingPhaseTracker.Instance == null || !VoidlingPhaseTracker.Instance.IsInFight)
                    return;

                switch (replacementType)
                {
                    case BossReplacementType.VoidlingPhase1:
                    case BossReplacementType.VoidlingPhase2:
                    case BossReplacementType.VoidlingPhase3:
#if DEBUG
                        Log.Debug($"Running {nameof(handleSpawnedVoidlingCharacterClient)}");
#endif
                        handleSpawnedVoidlingCharacterClient(masterObject, (int)(replacementType - BossReplacementType.VoidlingPhase1) + 1);
                        break;
                }
            }

            static void handleSpawnedVoidlingCharacterClient(GameObject masterObject, int phase)
            {
                VoidlingReplacement voidlingReplacement = masterObject.AddComponent<VoidlingReplacement>();
                voidlingReplacement.Phase = phase;
                voidlingReplacement.Initialize();
            }

            static void handleSpawnedVoidlingCharacterServer(SpawnCard.SpawnResult spawnResult)
            {
                if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeVoidling && VoidlingPhaseTracker.Instance != null && VoidlingPhaseTracker.Instance.IsInFight)
                {
                    VoidlingReplacement voidlingReplacement = spawnResult.spawnedInstance.AddComponent<VoidlingReplacement>();
                    voidlingReplacement.Phase = VoidlingPhaseTracker.Instance.Phase;
                    voidlingReplacement.Initialize();
                }
            }
        }
    }
}
