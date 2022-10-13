#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public enum HoldoutZoneStateType : byte
    {
        Invalid,
        Idle,
        IdleToCharging,
        Charging,
        Charged,
        Finished
    }
}
#endif