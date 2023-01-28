using BepInEx;
using R2API;
using R2API.Utils;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.CustomContent;
using RoR2Randomizer.Networking;
using RoR2Randomizer.RandomizerControllers;
using RoR2Randomizer.Utility;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: NetworkCompatibility]

namespace RoR2Randomizer
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(Constants.RISK_OF_OPTIONS_GUID)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "RoR2Randomizer";
        public const string PluginVersion = "0.8.4";

        public static Main Instance { get; private set; }

        void Awake()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Log.Init(Logger);

            Instance = this;

            CustomNetworkMessageManager.RegisterMessages();

            if (ModCompatibility.RiskOfOptionsCompat.IsEnabled)
                ModCompatibility.RiskOfOptionsCompat.Setup();

            ConfigManager.Initialize(Config);

            InitializationManager.Init();

            new ContentPackManager().Init();

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F1} seconds");
        }

        void OnDestroy()
        {
            InitializationManager.Cleanup();

            CharacterReplacements.Uninitialize();

            NetworkingManager.Uninitialize();
        }
    }
}
