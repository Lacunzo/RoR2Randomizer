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
    }
}
