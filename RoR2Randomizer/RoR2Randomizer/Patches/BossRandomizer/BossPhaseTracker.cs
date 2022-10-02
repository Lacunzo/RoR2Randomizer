using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public class BossPhaseTracker<T> : BossTracker<T> where T : BossPhaseTracker<T>
    {
        uint _phase;

        public uint Phase
        {
            get
            {
                return _phase;
            }
            protected set
            {
                if (MiscUtils.TryAssign(ref _phase, value))
                {
#if DEBUG
                    Log.Debug($"Enter {_debugName} fight phase {value}");
#endif

                    OnPhaseChanged?.Invoke();
                }
            }
        }

        public event Action OnPhaseChanged;

        protected BossPhaseTracker(string debugName) : base(debugName)
        {
        }
    }
}
