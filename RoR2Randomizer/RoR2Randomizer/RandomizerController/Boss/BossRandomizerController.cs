// #define MITHRIX_REPLACEMENTS_MODEL_SCALING

using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Patches.BossRandomizer.Mithrix;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public class BossRandomizerController : Singleton<BossRandomizerController>
    {
        // NOTES:
        // ArchWispMaster: No localized name
        // BeetleCrystalMaster: No localized name
        // BeetleGuardMasterCrystal: No localized name, NullRef in CharacterModel.UpdateMaterials on spawn
        // MajorConstructMaster: No localized name or subtitle
        // MiniVoidRaidCrabMasterPhase1: Spawns in the ground, only in phase 1 however
        // EntityStates.VoidRaidCrab.EscapeDeath has NullRef in OnExit (most likely VoidRaidGauntletController.instance)

        static readonly InitializeOnAccess<GameObject[]> _availableMasterObjects = new InitializeOnAccess<GameObject[]>(() =>
        {
            return MasterCatalog.masterPrefabs.Where(master =>
            {
                if (!master || !master.GetComponent<CharacterMaster>())
                    return false;

                switch (master.name)
                {
                    case "AncientWispMaster": // Does nothing
                    case "ArtifactShellMaster": // No model, does not attack, cannot be damaged
                    case "BrotherHauntMaster": // No model
                    case "ClaymanMaster": // No hitboxes
                    case "EngiBeamTurretMaster": // Seems to ignore the player
                    case "MinorConstructAttachableMaster": // Instantly dies
                    case "PlayerMaster": // Does not exist
                    case "RailgunnerMaster": // Does not exist
                    case "VoidRaidCrabJointMaster": // Balls
                    case "VoidRaidCrabMaster": // Beta voidling, half invisible
                        return false;
                }

#if MITHRIX_REPLACEMENTS_MODEL_SCALING && DEBUG
                Mithrix.LogScalingForMasterPrefab(master);
#endif

                return true;
            }).Distinct().ToArray();
        });

        static readonly InitializeOnAccess<EquipmentIndex[]> _availableDroneEquipments = new InitializeOnAccess<EquipmentIndex[]>(() =>
        {
            return EquipmentCatalog.equipmentDefs.Where(eq => eq && (eq.canDrop || eq.name == "BossHunterConsumed"))
                                                 .Select(e => e.equipmentIndex)
                                                 .ToArray();
        });

        void Update()
        {
            Mithrix.Update();
        }

        public static class Mithrix
        {
#if DEBUG
            public enum DebugMode : byte
            {
                None,
                Manual,
                Forced
            }

            static DebugMode debugMode => ConfigManager.BossRandomizer.MithrixDebugMode;
#endif

#if MITHRIX_REPLACEMENTS_MODEL_SCALING && DEBUG
            public static void LogScalingForMasterPrefab(GameObject master)
            {
                CharacterBody body = master.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<CharacterBody>();

                float scaleFactor = MainMithrixReplacement.GetModelScaleFactor(body);
                if (scaleFactor != 1f)
                {
                    Log.Debug($"Will scale {body.GetDisplayName()} by {scaleFactor}");
                }
            }
#endif

            class BaseMithrixReplacement : MonoBehaviour
            {
                protected CharacterMaster _master;

                public virtual void Initialize()
                {
                    _master = GetComponent<CharacterMaster>();

                    CharacterBody body = _master.GetBody();
                    if (body)
                    {
                        if (body.bodyIndex == BodyCatalog.FindBodyIndex("EquipmentDroneBody"))
                        {
                            Inventory inventory = _master.inventory;
                            if (inventory && inventory.GetEquipmentIndex() == EquipmentIndex.None)
                            {
                                EquipmentIndex equipment = _availableDroneEquipments.Get.GetRandomOrDefault(EquipmentIndex.None);
                                inventory.SetEquipmentIndex(equipment);

#if DEBUG
                                Log.Debug($"Gave {Language.GetString(EquipmentCatalog.GetEquipmentDef(equipment).nameToken)} to {Language.GetString(body.baseNameToken)}");
#endif
                            }
                        }
                        else if (body.bodyIndex == BodyCatalog.FindBodyIndex("DroneCommanderBody")) // Col. Droneman
                        {
                            Inventory inventory = _master.inventory;
                            if (inventory)
                            {
                                const int NUM_DRONE_PARTS = 1;

                                Patches.Reverse.DroneWeaponsBehavior.SetNumDroneParts(inventory, NUM_DRONE_PARTS);

#if DEBUG
                                Log.Debug($"Gave {NUM_DRONE_PARTS} drone parts to {Language.GetString(body.baseNameToken)}");
#endif
                            }
                        }
                    }
                }
            }

            class MainMithrixReplacement : BaseMithrixReplacement
            {
                public bool IsHurt;

#if MITHRIX_REPLACEMENTS_MODEL_SCALING
                float? _modelScaleFactor;
#endif

                public override void Initialize()
                {
                    base.Initialize();

                    CharacterBody body = _master.GetBody();
                    if (body)
                    {
                        if (string.IsNullOrEmpty(body.subtitleNameToken))
                        {
                            body.subtitleNameToken = "BROTHER_BODY_SUBTITLE";
                        }

                        if (IsHurt)
                        {
                            // Prevent low-health replacements from instantly dying
                            body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, SpellChannelEnterState.duration);

                            EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                            if (bodyState)
                            {
                                bodyState.initialStateType = _mithrixHurtInitialState;
                            }
                            else
                            {
                                Log.Warning($"Body entityState for {body.GetDisplayName()} could not be found!");
                            }

                            if (!body.GetComponent<ReturnStolenItemsOnGettingHit>())
                            {
                                ReturnStolenItemsOnGettingHit newReturnItems = body.gameObject.AddComponent<ReturnStolenItemsOnGettingHit>();

                                HealthComponent bodyHealthComponent = body.healthComponent;

                                newReturnItems.healthComponent = bodyHealthComponent;

                                ReturnStolenItemsOnGettingHit mithrixReturnItems = _mithrixReturnItemsComponent.Get;
                                newReturnItems.minPercentagePerItem = mithrixReturnItems.minPercentagePerItem;
                                newReturnItems.maxPercentagePerItem = mithrixReturnItems.maxPercentagePerItem;

                                newReturnItems.initialPercentageToFirstItem = mithrixReturnItems.initialPercentageToFirstItem;

                                // Re-run awake
                                newReturnItems.Awake();

                                MiscUtils.AddItem(ref bodyHealthComponent.onTakeDamageReceivers, newReturnItems);
                            }
                        }

#if MITHRIX_REPLACEMENTS_MODEL_SCALING
                        if (body.modelLocator)
                        {
                            Transform modelTransform = body.modelLocator.modelTransform;
                            if (modelTransform)
                            {
                                float scaleFactor = GetModelScaleFactor(body);
                                if (scaleFactor != 1f)
                                {
                                    _modelScaleFactor = scaleFactor;
                                    modelTransform.localScale *= scaleFactor;

                                    body.modelLocator.onModelChanged += ModelLocator_onModelChanged;

#if DEBUG
                                    Log.Debug($"MainMithrixReplacement: Scaled {body.GetDisplayName()} by {scaleFactor}");
#endif
                                }
                            }
                        }
#endif
                    }
                }

#if MITHRIX_REPLACEMENTS_MODEL_SCALING
                void OnDestroy()
                {
                    if (_modelScaleFactor.HasValue && _master)
                    {
                        CharacterBody body = _master.GetBody();
                        if (body && body.modelLocator)
                        {
                            body.modelLocator.onModelChanged -= ModelLocator_onModelChanged;
                        }
                    }
                }

                void ModelLocator_onModelChanged(Transform newModel)
                {
                    if (newModel && _modelScaleFactor.HasValue)
                    {
                        newModel.localScale *= _modelScaleFactor.Value;
                    }
                }

                public static float GetModelScaleFactor(CharacterBody body)
                {
                    if (body.modelLocator)
                    {
                        if (body.TryGetComponent<ModelLocator>(out ModelLocator modelLocator) && modelLocator._modelTransform)
                        {
                            if (modelLocator._modelTransform.TryGetComponent<CharacterModel>(out CharacterModel characterModel))
                            {
                                if (characterModel.TryGetModelBounds(out Bounds bounds))
                                {
                                    const float MAX_SIZE_TO_SCALE = 20f;
                                    const float MAX_SIZE_TO_SCALE_SQR = MAX_SIZE_TO_SCALE * MAX_SIZE_TO_SCALE;

                                    float sqrMagnitude = bounds.size.sqrMagnitude;
                                    if (sqrMagnitude <= MAX_SIZE_TO_SCALE_SQR)
                                    {
                                        const float MAX_SCALE_FACTOR = 3f;

                                        // Scale factor of MAX_SCALE_FACTOR when sqrMagnitude is 0, and 1 when sqrMagnitude is MAX_SIZE_TO_SCALE_SQR
                                        return (-(MAX_SCALE_FACTOR - 1f) / MAX_SIZE_TO_SCALE_SQR * sqrMagnitude) + MAX_SCALE_FACTOR;
                                    }
                                }
                            }
                        }
                    }

                    return 1f;
                }
#endif
            }

            class MithrixPhase2EnemiesReplacement : BaseMithrixReplacement
            {
            }

            static readonly SerializableEntityStateType _mithrixHurtInitialState = new SerializableEntityStateType(typeof(SpellChannelEnterState));

            static readonly InitializeOnAccess<ReturnStolenItemsOnGettingHit> _mithrixReturnItemsComponent = new InitializeOnAccess<ReturnStolenItemsOnGettingHit>(() =>
            {
                return Caches.MasterPrefabs["BrotherHurtMaster"].bodyPrefab.GetComponent<ReturnStolenItemsOnGettingHit>();
            });

            public static bool TryGetOverridePrefabFor(SpawnCard card, out GameObject overridePrefab)
            {
                if (ConfigManager.BossRandomizer.Enabled && MithrixPhaseTracker.IsInMithrixFight)
                {
                    if ((ConfigManager.BossRandomizer.RandomizeMithrix && (card == SpawnCardTracker.MithrixNormalSpawnCard || card == SpawnCardTracker.MithrixHurtSpawnCard))
                     || (ConfigManager.BossRandomizer.RandomizeMithrixPhase2 && SpawnCardTracker.IsPartOfMithrixPhase2(card)))
                    {
#if DEBUG
                        switch (debugMode)
                        {
                            case DebugMode.Manual:
                                overridePrefab = MasterCatalog.masterPrefabs[_masterIndex];
                                break;
                            case DebugMode.Forced:
                                overridePrefab = MasterCatalog.FindMasterPrefab(ConfigManager.BossRandomizer.DebugMithrixForcedMasterName);
                                break;
                            case DebugMode.None:
                                default:
#endif
                                overridePrefab = _availableMasterObjects.Get.GetRandomOrDefault();
#if DEBUG
                                break;
                        }
#endif

#if DEBUG
                        Log.Debug($"MithrixRandomizer: Replaced {card.prefab.name} with {overridePrefab?.name ?? "null"}");
#endif

                        return (bool)overridePrefab;
                    }
                }

                overridePrefab = null;
                return false;
            }

            public static void HandleSpawnedMithrixCharacter(SpawnCard.SpawnResult spawnResult)
            {
                if (ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled && MithrixPhaseTracker.IsInMithrixFight)
                {
                    BaseMithrixReplacement baseMithrixReplacement = null;
                    if (MithrixPhaseTracker.Phase == 2)
                    {
                        if (ConfigManager.BossRandomizer.RandomizeMithrixPhase2 && SpawnCardTracker.IsPartOfMithrixPhase2(spawnResult.spawnRequest.spawnCard))
                        {
                            baseMithrixReplacement = spawnResult.spawnedInstance.AddComponent<MithrixPhase2EnemiesReplacement>();
                        }
                    }
                    else
                    {
                        if (ConfigManager.BossRandomizer.RandomizeMithrix)
                        {
                            bool isHurtMithrixReplacement;
                            if ((isHurtMithrixReplacement = spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixHurtSpawnCard) ||
                                spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixNormalSpawnCard)
                            {
                                MainMithrixReplacement mainMithrixReplacement = spawnResult.spawnedInstance.AddComponent<MainMithrixReplacement>();
                                mainMithrixReplacement.IsHurt = isHurtMithrixReplacement;

                                baseMithrixReplacement = mainMithrixReplacement;
                            }
                        }
                    }

                    if (baseMithrixReplacement)
                    {
                        baseMithrixReplacement.Initialize();
                    }
                }
            }

            public static bool IsReplacedMithrix(GameObject master)
            {
                return master.GetComponent<MainMithrixReplacement>();
            }

            public static bool IsReplacedMithrixPhase2Spawn(GameObject master)
            {
                return master.GetComponent<MithrixPhase2EnemiesReplacement>();
            }

            public static bool IsReplacedPartOfMithrixFight(GameObject master)
            {
                return IsReplacedMithrix(master) || IsReplacedMithrixPhase2Spawn(master);
            }

#if DEBUG
            static int _masterIndex = 0;
#endif

            public static void Update()
            {
#if DEBUG
                if (debugMode == DebugMode.Manual && ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled)
                {
                    bool changedMasterIndex = false;
                    if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    {
                        if (++_masterIndex >= MasterCatalog.masterPrefabs.Length)
                            _masterIndex = 0;

                        changedMasterIndex = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                    {
                        if (--_masterIndex < 0)
                            _masterIndex = MasterCatalog.masterPrefabs.Length - 1;

                        changedMasterIndex = true;
                    }

                    if (changedMasterIndex)
                    {
                        Log.Debug($"Current Mithrix override: {MasterCatalog.masterPrefabMasterComponents[_masterIndex].name} ({_masterIndex})");
                    }
                }
#endif
            }
        }
    }
}
