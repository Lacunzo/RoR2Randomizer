using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
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
    public class Main : BaseUnityPlugin, IUnityCallbackProvider
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "RoR2Randomizer";
        public const string PluginVersion = "0.0.1";

        public const bool SKILL_RANDOMIZER_ENABLED = true;
        public const bool MITHRIX_RANDOMIZER_ENABLED = true;

        public static Main Instance { get; private set; }

        readonly HandledList<Action> _updateCallbacks = new HandledList<Action>(0, 1);

        public int RegisterUpdateCallback(Action update)
        {
            if (update is null)
                throw new ArgumentNullException(nameof(update));

            return _updateCallbacks.Store(update);
        }

        public void UnregisterUpdateCallback(int handle)
        {
            _updateCallbacks.Clear(handle);
        }

        void Awake()
        {
            Instance = this;

            Log.Init(Logger);

            PatchController.Setup();
        }

        void OnDestroy()
        {
            PatchController.Cleanup();
        }

        void Update()
        {
            foreach (Action callback in _updateCallbacks)
            {
                callback?.Invoke();
            }

#if DEBUG
            if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                CharacterMaster playerMaster = PlayerCharacterMasterController.instances[0].master;
                playerMaster.inventory.GiveRandomItems(100, false, false);
                playerMaster.inventory.SetEquipmentIndex(RoR2Content.Equipment.Gateway.equipmentIndex); // Vase
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName("moon2"));
            }
#endif
        }
    }
}
