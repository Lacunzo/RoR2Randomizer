using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public class BossPhaseTracker<T> : BossTracker<T> where T : BossPhaseTracker<T>
    {
        public TrackedValue<uint> Phase = new TrackedValue<uint>();
    }
}
