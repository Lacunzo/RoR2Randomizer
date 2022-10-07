using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Voidling;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using EntityStates;
using UnityModdingUtility;
using RoR2.ContentManagement;
using System.Linq;
using System;
using HG.GeneralSerializer;
using HarmonyLib;

namespace RoR2Randomizer.RandomizerControllers.Boss
{
    public partial class BossRandomizerController : MonoBehaviour
    {
        public static class Voidling
        {
            public static readonly SerializableEntityStateType EscapeDeathState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.EscapeDeath));
            public static readonly SerializableEntityStateType FinalDeathState = new SerializableEntityStateType(typeof(EntityStates.VoidRaidCrab.DeathState));

            public static readonly InitializeOnAccess<float> FinalDeathStateAnimationDuration = new InitializeOnAccess<float>(() =>
            {
                Type deathStateType = FinalDeathState.stateType;
                EntityStateConfiguration finalDeathStateConfig = ContentManager.entityStateConfigurations.Single(c => (Type)c.targetType == deathStateType);

                SerializedField durationField = finalDeathStateConfig.serializedFieldsCollection.serializedFields.Single(f => f.fieldName.Equals(nameof(EntityStates.VoidRaidCrab.DeathState.duration)));

                return (float)durationField.fieldValue.GetValue(AccessTools.DeclaredField(typeof(EntityStates.VoidRaidCrab.DeathState), nameof(EntityStates.VoidRaidCrab.DeathState.duration)));
            });

            public static void Initialize()
            {
                if (VoidlingPhaseTracker.Instance != null)
                {
                    VoidlingPhaseTracker.Instance.IsInFight.OnChanged += IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive += SyncBossReplacementCharacter_OnReceive;

                SceneCatalog.onMostRecentSceneDefChanged += SceneCatalog_onMostRecentSceneDefChanged;
            }

            public static void Uninitialize()
            {
                if (VoidlingPhaseTracker.Instance != null)
                {
                    VoidlingPhaseTracker.Instance.IsInFight.OnChanged -= IsInFight_OnChanged;
                }

                SyncBossReplacementCharacter.OnReceive -= SyncBossReplacementCharacter_OnReceive;

                SceneCatalog.onMostRecentSceneDefChanged -= SceneCatalog_onMostRecentSceneDefChanged;
            }

            static void IsInFight_OnChanged(bool isInFight)
            {
                if (NetworkServer.active)
                {
                    if (isInFight)
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = (ref SpawnCard card, out GenericScriptedSpawnHook.ResetCardDelegate resetCardFunc) =>
                        {
                            if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeVoidling && SpawnCardTracker.IsAnyVoidlingPhase(card))
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

                        GenericScriptedSpawnHook.OnSpawned += handleSpawnedVoidlingCharacterServer;
                    }
                    else
                    {
                        GenericScriptedSpawnHook.OverrideSpawnPrefabFunc = null;
                        GenericScriptedSpawnHook.OnSpawned -= handleSpawnedVoidlingCharacterServer;
                    }
                }
            }

            static void SceneCatalog_onMostRecentSceneDefChanged(SceneDef obj)
            {
                if (ConfigManager.BossRandomizer.Enabled && ConfigManager.BossRandomizer.RandomizeVoidling && obj.cachedName == Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME)
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
                        handleSpawnedVoidlingCharacterClient(masterObject, replacementType - BossReplacementType.VoidlingPhase1 + 1);
                        break;
                }
            }

            static void handleSpawnedVoidlingCharacterClient(GameObject masterObject, uint phase)
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
