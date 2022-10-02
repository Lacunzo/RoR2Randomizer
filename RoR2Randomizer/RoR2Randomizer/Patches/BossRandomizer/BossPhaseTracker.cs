using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public class BossPhaseTracker<T> : BossTracker<T> where T : BossPhaseTracker<T>
    {
        public TrackedValue<uint> Phase = new TrackedValue<uint>();
    }
}
