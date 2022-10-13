#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public struct HoldoutZoneStateCollection
    {
        static readonly SerializableEntityStateType _defaultState = new SerializableEntityStateType(typeof(Idle));

        public SerializableEntityStateType Idle = _defaultState;
        public SerializableEntityStateType IdleToCharging = _defaultState;
        public SerializableEntityStateType Charging = _defaultState;
        public SerializableEntityStateType Charged = _defaultState;
        public SerializableEntityStateType Finished = _defaultState;

        public HoldoutZoneStateCollection()
        {
        }
    }
}
#endif