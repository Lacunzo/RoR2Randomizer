#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.HoldoutZoneRandomizer;
using RoR2Randomizer.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    [RandomizerController]
    public sealed class HoldoutZoneRandomizerController : MonoBehaviour
    {
        static readonly RunSpecific<bool> _holdoutZoneReplacementsReceivedFromServer = new RunSpecific<bool>();

        static readonly RunSpecific<ReplacementDictionary<HoldoutZoneType>> _holdoutZoneReplacements = new RunSpecific<ReplacementDictionary<HoldoutZoneType>>((out ReplacementDictionary<HoldoutZoneType> result) =>
        {
            if (NetworkServer.active && ConfigManager.HoldoutZoneRandomizer.Enabled)
            {
                result = ReplacementDictionary<HoldoutZoneType>.CreateFrom(_zoneInfos.Select(zi => zi.Key));

                new SyncHoldoutZoneReplacements(result).Send(NetworkDestination.Clients);

                return true;
            }

            result = null;
            return false;
        });

        static Dictionary<HoldoutZoneType, HoldoutZoneInfo> _zoneInfos;

        public static bool IsActive => ((NetworkServer.active && ConfigManager.HoldoutZoneRandomizer.Enabled) || (NetworkClient.active && _holdoutZoneReplacementsReceivedFromServer)) && _holdoutZoneReplacements.HasValue;

        static void onSyncHoldoutZoneReplacementsReceivedFromServer(ReplacementDictionary<HoldoutZoneType> zoneReplacements)
        {
            _holdoutZoneReplacements.Value = zoneReplacements;
            _holdoutZoneReplacementsReceivedFromServer.Value = _holdoutZoneReplacements.HasValue;
        }

        void Awake()
        {
            SyncHoldoutZoneReplacements.OnReceive += onSyncHoldoutZoneReplacementsReceivedFromServer;

            if (_zoneInfos == null)
            {
                List<HoldoutZoneInfo> zones = new List<HoldoutZoneInfo>();

                void tryAddZone(HoldoutZoneType type, string assetPath)
                {
                    if (HoldoutZoneInfo.TryGetHoldoutZoneInfoFromAsset(assetPath, type, out HoldoutZoneInfo zone))
                    {
                        zones.Add(zone);
                    }
                }

                //tryAddZone(HoldoutZoneType.InfiniteTowerSafeWard, "Prefabs/NetworkedObjects/InfiniteTowerSafeWard/InfiniteTowerSafeWard");
                tryAddZone(HoldoutZoneType.InfiniteTowerSafeWard, "Prefabs/NetworkedObjects/InfiniteTowerSafeWard/InfiniteTowerSafeWardAwaitingInteraction");
                tryAddZone(HoldoutZoneType.MoonBatteryBlood, "Prefabs/NetworkedObjects/MoonBatteryBlood");
                tryAddZone(HoldoutZoneType.MoonBatteryDesign, "Prefabs/NetworkedObjects/MoonBatteryDesign");
                tryAddZone(HoldoutZoneType.MoonBatteryMass, "Prefabs/NetworkedObjects/MoonBatteryMass");
                tryAddZone(HoldoutZoneType.MoonBatterySoul, "Prefabs/NetworkedObjects/MoonBatterySoul");
                tryAddZone(HoldoutZoneType.NullSafeWard, "Prefabs/NetworkedObjects/NullSafeWard");
                tryAddZone(HoldoutZoneType.LunarTeleporter, "Prefabs/NetworkedObjects/Teleporters/LunarTeleporter Variant");
                tryAddZone(HoldoutZoneType.Teleporter, "Prefabs/NetworkedObjects/Teleporters/Teleporter1");

                _zoneInfos = zones.ToDictionary(zi => zi.ZoneType);
            }

            foreach (HoldoutZoneInfo zoneInfo in _zoneInfos.Values)
            {
                zoneInfo.AddZoneIdentifierToPrefabIfMissing();
            }
        }

        void OnDestroy()
        {
            SyncHoldoutZoneReplacements.OnReceive -= onSyncHoldoutZoneReplacementsReceivedFromServer;

            _holdoutZoneReplacements.Dispose();
            _holdoutZoneReplacementsReceivedFromServer.Dispose();
        }

        static bool tryGetZoneIndentifier(HoldoutZoneController controller, out HoldoutZoneIdentifier identifier)
        {
            if (controller.TryGetComponent<HoldoutZoneIdentifier>(out identifier))
                return true;

            SceneDef mostRecentScene = SceneCatalog.mostRecentSceneDef;
            if (mostRecentScene)
            {
                string currentSceneName = mostRecentScene.cachedName;
                if (currentSceneName == Constants.SceneNames.VOID_FIELDS_SCENE_NAME)
                {
                    Transform parent = controller.transform.parent;
                    if (parent && parent.GetComponent<ArenaMissionController>())
                    {
                        identifier = HoldoutZoneIdentifier.AddIdentifier(controller, HoldoutZoneType.NullSafeWard);
                        return true;
                    }
                }
                else if (currentSceneName == Constants.SceneNames.COMMENCEMENT_SCENE_NAME)
                {
                    Transform parent = controller.transform.parent;
                    if (parent)
                    {
                        string name = parent.name;

                        string pillarName = name.Substring(name.LastIndexOf(' ') + 1);
                        HoldoutZoneType zoneType = pillarName.ToLower() switch
                        {
                            "soul" => HoldoutZoneType.MoonBatterySoul,
                            "mass" => HoldoutZoneType.MoonBatteryMass,
                            "design" => HoldoutZoneType.MoonBatteryDesign,
                            "blood" => HoldoutZoneType.MoonBatteryBlood,
                            _ => HoldoutZoneType.Invalid
                        };

                        if (zoneType == HoldoutZoneType.Invalid)
                        {
                            Log.Warning($"Could not find zone type for pillar: '{controller}' ({pillarName})");
                        }
                        else
                        {
                            identifier = HoldoutZoneIdentifier.AddIdentifier(controller, zoneType);
                            return true;
                        }
                    }
                }
            }

            Log.Warning($"Holdout zone {controller} has no identifier");

            return false;
        }

        public static void TryRandomizeHoldoutZone(HoldoutZoneController controller)
        {
            if (IsActive && tryGetZoneIndentifier(controller, out HoldoutZoneIdentifier originalTypeIdentifier))
            {
                originalTypeIdentifier.InitializeZoneInfoIfNeeded();

                if (_holdoutZoneReplacements.Value.TryGetReplacement(originalTypeIdentifier.ZoneInfo.ZoneType, out HoldoutZoneType replacementType))
                {
                    replacementType = HoldoutZoneType.InfiniteTowerSafeWard;

                    if (_zoneInfos.TryGetValue(replacementType, out HoldoutZoneInfo replacementZoneInfo) && replacementZoneInfo.ControllerPrefab)
                    {
                        if (originalTypeIdentifier.ZoneInfo.ZoneType == replacementZoneInfo.ZoneType || controller.GetComponent<RandomizedHoldoutZoneController>())
                            return;

#if DEBUG
                        Log.Debug($"Holdout Zone Randomizer: Replaced zone {originalTypeIdentifier.ZoneInfo.ZoneType} ({controller.name}) -> {replacementType} ({replacementZoneInfo.ControllerPrefab.name})");
#endif

                        RandomizedHoldoutZoneController randomizedHoldoutZoneController = controller.gameObject.AddComponent<RandomizedHoldoutZoneController>();
                        randomizedHoldoutZoneController.Initialize(originalTypeIdentifier.ZoneInfo, replacementZoneInfo);
                    }
                }
            }
        }

        public static HoldoutZoneInfo GetHoldoutZone(HoldoutZoneType type)
        {
            return _zoneInfos[type];
        }

#if DEBUG
        void Update()
        {
            if (Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.P))
            {
                if (!_isScanningResources)
                {
                    _scanResourcesForHoldoutZonesCoroutine = StartCoroutine(scanResourcesForHoldoutZones());
                }
                else
                {
                    StopCoroutine(_scanResourcesForHoldoutZonesCoroutine);
                    _scanResourcesForHoldoutZonesCoroutine = null;
                    _isScanningResources = false;
                }
            }
        }

        Coroutine _scanResourcesForHoldoutZonesCoroutine;
        bool _isScanningResources;
        IEnumerator scanResourcesForHoldoutZones()
        {
            _isScanningResources = true;

            const int RESET_COUNT = 5;
            int count = 0;

            List<string> paths = new List<string>();
            LegacyResourcesAPI.GetAllPaths(paths);
            for (int i = 0; i < paths.Count; i++)
            {
                // Cause infinite loading
                switch (paths[i])
                {
                    case "Button_Off":
                    case "Button_On":
                    case "Icon":
                    case "Missing Object":
                    case "Materials/Collider":
                        continue;
                }

                Log.Debug($"Scanning path {paths[i]} {i + 1}/{paths.Count} ({(float)(i + 1) / paths.Count * 100f:F3}%)");

                UnityEngine.Object obj = LegacyResourcesAPI.Load<UnityEngine.Object>(paths[i]);
                if (obj)
                {
                    if (obj is SpawnCard card)
                    {
                        obj = card.prefab;
                    }

                    if (obj is Component || obj is GameObject)
                    {
                        if ((obj is Component c && c.GetComponentInChildren<HoldoutZoneController>()) || (obj is GameObject go && go.GetComponentInChildren<HoldoutZoneController>()))
                        {
                            Log.Debug($"Holdout zone exists in '{paths[i]}'");
                        }
                    }
                    else
                    {
                        Log.Debug($"Unhandled object type {(obj ? obj.GetType().FullName : "null")}");
                    }
                }

                Log.Debug($"Scanned path {paths[i]} {i+1}/{paths.Count} ({(float)(i + 1) / paths.Count * 100f:F3}%)");

                if (++count >= RESET_COUNT)
                {
                    yield return 0;
                    count = 0;
                }
            }

            Log.Debug("Scanning finished");

            _scanResourcesForHoldoutZonesCoroutine = null;
            _isScanningResources = false;
        }
#endif
    }
}
#endif