using EntityStates;
using EntityStates.BrotherMonster;
using R2API;
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

        static readonly Dictionary<string, string> _characterNamesLanguageAdditions = new Dictionary<string, string>
        {
            { "ARCHWISP_BODY_NAME", "Arch Wisp" },

            { "BEETLE_CRYSTAL_BODY_NAME", "Crystal Beetle" },

            { "MAJORCONSTRUCT_BODY_NAME", "Major Construct" },
            { "MAJORCONSTRUCT_BODY_SUBTITLE", "Defense System" }
        };

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
                    return _availableMasterObjects.Get[_masterIndex];
                case DebugMode.Forced:
                    return MasterCatalog.FindMasterPrefab(ConfigManager.BossRandomizer.DebugBossForcedMasterName);
            }
#endif
            return _availableMasterObjects.Get.GetRandomOrDefault();
        }

        public static bool IsReplacedBossCharacter(GameObject masterObject)
        {
            return masterObject && masterObject.GetComponent<BaseBossReplacement>();
        }

        protected override void Awake()
        {
            base.Awake();

            LanguageAPI.Add(_characterNamesLanguageAdditions);

            Mithrix.Initialize();
            Voidling.Initialize();
            Aurelionite.Initialize();
        }

        void OnDestroy()
        {
            Mithrix.Uninitialize();
            Voidling.Uninitialize();
            Aurelionite.Uninitialize();
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
                    if (++_masterIndex >= _availableMasterObjects.Get.Length)
                        _masterIndex = 0;

                    changedMasterIndex = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_masterIndex < 0)
                        _masterIndex = _availableMasterObjects.Get.Length - 1;

                    changedMasterIndex = true;
                }

                if (changedMasterIndex)
                {
                    if (ConfigManager.BossRandomizer.RandomizeAurelionite)
                    {
                        Aurelionite.OnDebugCharacterChanged();
                    }

                    Log.Debug($"Current boss override: {_availableMasterObjects.Get[_masterIndex].name} ({_masterIndex})");
                }
            }
        }
#endif
    }
}
