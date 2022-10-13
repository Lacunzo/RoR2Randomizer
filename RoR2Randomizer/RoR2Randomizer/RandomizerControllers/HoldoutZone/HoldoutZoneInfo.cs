#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using RoR2;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public readonly struct HoldoutZoneInfo
    {
        public readonly HoldoutZoneType ZoneType;
        public readonly HoldoutZoneController ControllerPrefab;

        public readonly bool SyncState;
        public readonly HoldoutZoneStateCollection StateCollection;

        public HoldoutZoneInfo(HoldoutZoneType zoneType, HoldoutZoneController controllerPrefab)
        {
            ZoneType = zoneType;
            ControllerPrefab = controllerPrefab;

            StateCollection = new HoldoutZoneStateCollection();
            switch (zoneType)
            {
                case HoldoutZoneType.InfiniteTowerSafeWard:
                    StateCollection.Idle = new SerializableEntityStateType(typeof(EntityStates.InfiniteTowerSafeWard.AwaitingActivation));
                    StateCollection.Charging = new SerializableEntityStateType(typeof(EntityStates.InfiniteTowerSafeWard.Active));
                    StateCollection.Charged = new SerializableEntityStateType(typeof(EntityStates.InfiniteTowerSafeWard.AwaitingActivation));
                    StateCollection.Finished = new SerializableEntityStateType(typeof(EntityStates.InfiniteTowerSafeWard.AwaitingPortalUse));

                    SyncState = false;
                    break;
                case HoldoutZoneType.MoonBatteryBlood:
                case HoldoutZoneType.MoonBatteryDesign:
                case HoldoutZoneType.MoonBatteryMass:
                case HoldoutZoneType.MoonBatterySoul:
                    StateCollection.Idle = new SerializableEntityStateType(typeof(EntityStates.Missions.Moon.MoonBatteryInactive));

                    StateCollection.Charging = new SerializableEntityStateType(zoneType switch
                    {
                        HoldoutZoneType.MoonBatteryBlood => typeof(EntityStates.Missions.Moon.MoonBatteryBloodActive),
                        HoldoutZoneType.MoonBatteryDesign => typeof(EntityStates.Missions.Moon.MoonBatteryDesignActive),
                        _ => typeof(EntityStates.Missions.Moon.MoonBatteryActive)
                    });
                    
                    StateCollection.Charged = new SerializableEntityStateType(typeof(EntityStates.Missions.Moon.MoonBatteryComplete));
                    StateCollection.Finished = new SerializableEntityStateType(typeof(EntityStates.Missions.Moon.MoonBatteryComplete));

                    SyncState = true;
                    break;
                case HoldoutZoneType.NullSafeWard:

                    StateCollection.Idle = new SerializableEntityStateType(typeof(EntityStates.Missions.Arena.NullWard.WardOnAndReady));

                    StateCollection.Charging = new SerializableEntityStateType(typeof(EntityStates.Missions.Arena.NullWard.Active));

                    StateCollection.Charged = new SerializableEntityStateType(typeof(EntityStates.Missions.Arena.NullWard.Complete));
                    StateCollection.Finished = new SerializableEntityStateType(typeof(EntityStates.Missions.Arena.NullWard.Complete));

                    SyncState = false;
                    break;
                case HoldoutZoneType.LunarTeleporter:
                case HoldoutZoneType.Teleporter:
                    StateCollection.Idle = new SerializableEntityStateType(typeof(TeleporterInteraction.IdleState));
                    StateCollection.IdleToCharging = new SerializableEntityStateType(typeof(TeleporterInteraction.IdleToChargingState));
                    StateCollection.Charging = new SerializableEntityStateType(typeof(TeleporterInteraction.ChargingState));
                    StateCollection.Charged = new SerializableEntityStateType(typeof(TeleporterInteraction.ChargedState));
                    StateCollection.Finished = new SerializableEntityStateType(typeof(TeleporterInteraction.FinishedState));

                    SyncState = true;
                    break;
            }
        }

        public void AddZoneIdentifierToPrefabIfMissing()
        {
            if (!ControllerPrefab.GetComponent<HoldoutZoneIdentifier>())
            {
                HoldoutZoneIdentifier.AddIdentifier(ControllerPrefab, ZoneType);
            }
        }

        public static bool TryGetHoldoutZoneInfoFromAsset(string assetPath, HoldoutZoneType type, out HoldoutZoneInfo zoneInfo)
        {
#if DEBUG
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void logWarning(string reason)
            {
                Log.Warning($"{nameof(HoldoutZoneInfo)} could not be created for {type} due to {reason} (path={assetPath})");
            }
#endif

            UnityEngine.Object asset = LegacyResourcesAPI.Load<UnityEngine.Object>(assetPath);
            if (!asset)
            {
#if DEBUG
                logWarning($"null asset");
#endif

                zoneInfo = default;
                return false;
            }

            GameObject prefab;
            if (asset is SpawnCard card)
            {
                prefab = card.prefab;
            }
            else if (asset is GameObject go)
            {
                prefab = go;
            }
            else
            {
                prefab = null;
            }

            if (!prefab)
            {
#if DEBUG
                logWarning($"null prefab");
#endif

                zoneInfo = default;
                return false;
            }

            HoldoutZoneController zoneController = prefab.GetComponentInChildren<HoldoutZoneController>();
            if (!zoneController)
            {
#if DEBUG
                logWarning($"no {nameof(HoldoutZoneController)} component found");
#endif

                zoneInfo = default;
                return false;
            }

            zoneInfo = new HoldoutZoneInfo(type, zoneController);
            return true;
        }
    }
}
#endif