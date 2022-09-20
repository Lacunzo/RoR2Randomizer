using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Patches;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;
using UnityModdingUtility;

namespace RoR2Randomizer
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Constants.RISK_OF_OPTIONS_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "RoR2Randomizer";
        public const string PluginVersion = "0.0.1";

        public static Main Instance { get; private set; }

        void Awake()
        {
            Log.Init(Logger);

            Instance = this;

            if (ModCompatibility.RiskOfOptionsCompat.IsEnabled)
                ModCompatibility.RiskOfOptionsCompat.Setup();

            ConfigManager.Initialize(Config);

            PatchController.Setup();
        }

        void OnDestroy()
        {
            PatchController.Cleanup();
        }

#if DEBUG
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                foreach (PlayerCharacterMasterController masterController in PlayerCharacterMasterController.instances)
                {
                    CharacterMaster playerMaster = masterController.master;

                    playerMaster.inventory.GiveRandomItems(100, false, false);
                    playerMaster.inventory.SetEquipmentIndex(RoR2Content.Equipment.Gateway.equipmentIndex); // Vase
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName("moon2"));
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Stage.instance.BeginAdvanceStage(Run.instance.nextStageScene);
            }
        }
#endif
    }
}
