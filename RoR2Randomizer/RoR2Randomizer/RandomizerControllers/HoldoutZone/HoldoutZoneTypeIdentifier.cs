#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using RoR2;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public sealed class HoldoutZoneIdentifier : MonoBehaviour
    {
        public HoldoutZoneType ZoneType;

        // For some reason the ZoneInfo field value does not persist from prefab to instantiated, but ZoneType does, requiring a lookup for each instance.
        bool _hasInitializedZoneInfo;
        public HoldoutZoneInfo ZoneInfo;

        public void InitializeZoneInfoIfNeeded()
        {
            if (_hasInitializedZoneInfo)
                return;

            ZoneInfo = HoldoutZoneRandomizerController.GetHoldoutZone(ZoneType);
            _hasInitializedZoneInfo = true;
        }

        public static HoldoutZoneIdentifier AddIdentifier(HoldoutZoneController controller, HoldoutZoneType type)
        {
            HoldoutZoneIdentifier typeIdentifier = controller.gameObject.AddComponent<HoldoutZoneIdentifier>();
            typeIdentifier.ZoneType = type;

            return typeIdentifier;
        }
    }
}
#endif