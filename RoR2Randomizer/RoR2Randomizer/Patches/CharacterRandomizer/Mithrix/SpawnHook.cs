// #define MANUAL_MITHRIX_MASTER_INDEX

using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.Patches.CharacterRandomizer.Mithrix
{
    public static class SpawnHook
    {
        public class MithrixReplacement : MonoBehaviour
        {
        }

        public class MithrixPhase2EnemiesReplacement : MonoBehaviour
        {
        }

        // NOTES:
        // ArchWispMaster: No localized name
        // BeetleCrystalMaster: No localized name
        // BeetleGuardMasterCrystal: No localized name, NullRef in CharacterModel.UpdateMaterials on spawn
        // MajorConstructMaster: No localized name or subtitle
        // MiniVoidRaidCrabMasterPhase1: Spawns in the ground, only in phase 1 however
        // EntityStates.VoidRaidCrab.EscapeDeath has NullRef in OnExit (most likely VoidRaidGauntletController.instance)

#if !MANUAL_MITHRIX_MASTER_INDEX
        public static readonly InitializeOnAccess<GameObject[]> AvailableMasterObjects = new InitializeOnAccess<GameObject[]>(() =>
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
                        return false;
                }

                return true;
            }).ToArray();
        });
#endif

        static readonly SerializableEntityStateType _mithrixHurtInitialState = new SerializableEntityStateType(typeof(EntityStates.BrotherMonster.SpellChannelEnterState));

        static readonly InitializeOnAccess<EquipmentIndex[]> _availableDroneEquipments = new InitializeOnAccess<EquipmentIndex[]>(() =>
        {
            return EquipmentCatalog.equipmentDefs.Where(eq =>
            {
                if (!eq)
                    return false;

                if (!eq.canDrop)
                {
                    switch (eq.name)
                    {
                        case "BossHunterConsumed": // Ahoy!
                        // Scrapped equipments
                        case "GhostGun":
                        case "IrradiatingLaser":
                        case "SoulJar":
                            break;
                        default:
                            return false;
                    }
                }

                return true;
            }).Select(e => e.equipmentIndex).ToArray();
        });

        static readonly InitializeOnAccess<ReturnStolenItemsOnGettingHit> _mithrixReturnItemsComponent = new InitializeOnAccess<ReturnStolenItemsOnGettingHit>(() =>
        {
            return Caches.MasterPrefabs["BrotherHurtMaster"].bodyPrefab.GetComponent<ReturnStolenItemsOnGettingHit>();
        });

#if MANUAL_MITHRIX_MASTER_INDEX
        static int _updateCallbackHandle = -1;
        static int _masterIndex = 0;
#endif

        public static void Apply()
        {
#if MANUAL_MITHRIX_MASTER_INDEX
            _updateCallbackHandle = Main.Instance.RegisterUpdateCallback(() =>
            {
                if (Main.MITHRIX_RANDOMIZER_ENABLED)
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
            });
#endif

            On.RoR2.ScriptedCombatEncounter.Spawn += ScriptedCombatEncounter_Spawn;
        }

        public static void Cleanup()
        {
#if MANUAL_MITHRIX_MASTER_INDEX
            Main.Instance.UnregisterUpdateCallback(_updateCallbackHandle);
            _updateCallbackHandle = -1;
#endif

            On.RoR2.ScriptedCombatEncounter.Spawn -= ScriptedCombatEncounter_Spawn;
        }

        static void ScriptedCombatEncounter_Spawn(On.RoR2.ScriptedCombatEncounter.orig_Spawn orig, ScriptedCombatEncounter self, ref ScriptedCombatEncounter.SpawnInfo spawnInfo)
        {
            if (!Main.MITHRIX_RANDOMIZER_ENABLED)
            {
                orig(self, ref spawnInfo);
                return;
            }

            GameObject originalPrefab = null;
            if (spawnInfo.spawnCard == SpawnCardTracker.MithrixNormalSpawnCard
                || spawnInfo.spawnCard == SpawnCardTracker.MithrixHurtSpawnCard
                || (SpawnCardTracker.Phase2SpawnCards != null && SpawnCardTracker.Phase2SpawnCards.Contains(spawnInfo.spawnCard)))
            {
                originalPrefab = spawnInfo.spawnCard.prefab;

                spawnInfo.spawnCard.prefab =
#if MANUAL_MITHRIX_MASTER_INDEX
                                             MasterCatalog.masterPrefabs[_masterIndex];
#else
                                             AvailableMasterObjects.Get.GetRandomOrDefault();
#endif
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
                if (!Main.MITHRIX_RANDOMIZER_ENABLED)
                    return;
                
                if (spawnResult.spawnRequest != null && spawnResult.spawnRequest.spawnCard)
                {
                    bool isHurtMithrixReplacement = spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixHurtSpawnCard;
                    bool isMithrixReplacement = isHurtMithrixReplacement || spawnResult.spawnRequest.spawnCard == SpawnCardTracker.MithrixNormalSpawnCard;
                    bool isPhase2Replacement = SpawnCardTracker.Phase2SpawnCards != null && SpawnCardTracker.Phase2SpawnCards.Contains(spawnResult.spawnRequest.spawnCard);
                    if (isMithrixReplacement || isPhase2Replacement)
                    {
                        if (spawnResult.spawnedInstance)
                        {
                            if (isMithrixReplacement)
                            {
                                spawnResult.spawnedInstance.AddComponent<MithrixReplacement>();
                            }
                            else if (isPhase2Replacement)
                            {
                                spawnResult.spawnedInstance.AddComponent<MithrixPhase2EnemiesReplacement>();
                            }

                            if (spawnResult.spawnedInstance.TryGetComponent<CharacterMaster>(out CharacterMaster master))
                            {
                                CharacterBody body = master.GetBody();

                                if (isMithrixReplacement)
                                {
                                    if (Run.instance.spawnRng.nextNormalizedFloat <= 0.3f)
                                    {
                                        const float SCALE_FACTOR_MIN = 0.5f;
                                        const float SCALE_FACTOR_MAX = 5f;

                                        float scaleFactor = (Mathf.Pow(Run.instance.spawnRng.nextNormalizedFloat, 2.5f) * (SCALE_FACTOR_MAX - SCALE_FACTOR_MIN)) + SCALE_FACTOR_MIN;

                                        body.modelLocator.modelTransform.localScale *= scaleFactor;
                                        body.modelLocator.onModelChanged += model =>
                                        {
                                            if (model)
                                            {
                                                model.localScale *= scaleFactor;
                                            }
                                        };
                                    }
                                }

                                if (isHurtMithrixReplacement)
                                {
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

                                        Array.Resize(ref bodyHealthComponent.onTakeDamageReceivers, bodyHealthComponent.onTakeDamageReceivers.Length + 1);
                                        bodyHealthComponent.onTakeDamageReceivers[bodyHealthComponent.onTakeDamageReceivers.Length - 1] = newReturnItems;
                                    }
                                }

                                if (body.bodyIndex == BodyCatalog.FindBodyIndex("EquipmentDrone"))
                                {
                                    body.master.inventory.SetEquipmentIndex(_availableDroneEquipments.Get.GetRandomOrDefault());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
