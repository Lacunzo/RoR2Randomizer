using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Patches.BossRandomizer.Mithrix;
using RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss
{
    public partial class BossRandomizerController : Singleton<BossRandomizerController>
    {
#if DEBUG
        public enum DebugMode : byte
        {
            None,
            Manual,
            Forced
        }

        static DebugMode debugMode => ConfigManager.BossRandomizer.BossDebugMode;
#endif

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

                return true;
            }).Distinct().ToArray();
        });

        public static readonly InitializeOnAccess<EquipmentIndex[]> AvailableDroneEquipments = new InitializeOnAccess<EquipmentIndex[]>(() =>
        {
            return EquipmentCatalog.equipmentDefs.Where(eq => eq && (eq.canDrop || eq.name == "BossHunterConsumed"))
                                                 .Select(e => e.equipmentIndex)
                                                 .ToArray();
        });

        static GameObject getBossOverrideMasterPrefab()
        {
#if DEBUG
            switch (debugMode)
            {
                case DebugMode.Manual:
                    return MasterCatalog.masterPrefabs[_masterIndex];
                case DebugMode.Forced:
                    return MasterCatalog.FindMasterPrefab(ConfigManager.BossRandomizer.DebugBossForcedMasterName);
                case DebugMode.None:
                default:
#endif
                    return _availableMasterObjects.Get.GetRandomOrDefault();
#if DEBUG
            }
#endif
        }

        public static bool IsReplacedBossCharacter(GameObject masterObject)
        {
            return masterObject && masterObject.GetComponent<BaseBossReplacement>();
        }

        protected override void Awake()
        {
            base.Awake();
            Mithrix.Initialize();
            Voidling.Initialize();
        }

        void OnDestroy()
        {
            Mithrix.Uninitialize();
            Voidling.Uninitialize();
        }

#if DEBUG
        static int _masterIndex = 0;

        void Update()
        {
            if (debugMode == DebugMode.Manual && ConfigManager.BossRandomizer.Enabled)
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
                    Log.Debug($"Current boss override: {MasterCatalog.masterPrefabMasterComponents[_masterIndex].name} ({_masterIndex})");
                }
            }
        }
#endif
    }
}
