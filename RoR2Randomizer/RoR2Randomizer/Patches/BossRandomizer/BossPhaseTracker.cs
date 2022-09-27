using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public class BossPhaseTracker<T> where T : BossPhaseTracker<T>
    {
        public static T Instance { get; private set; }

#if DEBUG
        readonly string _debugName;
#endif

        bool _isInFight;
        public bool IsInFight
        {
            get
            {
                return _isInFight;
            }
            protected set
            {
                if (MiscUtils.TryAssign(ref _isInFight, value))
                {
                    if (value)
                    {
#if DEBUG
                        Log.Debug($"Enter {_debugName} fight");
#endif

                        OnEnterFight?.Invoke();
                    }
                    else
                    {
#if DEBUG
                        Log.Debug($"Exit {_debugName} fight");
#endif

                        OnExitFight?.Invoke();
                    }
                }
            }
        }

        public event Action OnEnterFight;
        public event Action OnExitFight;

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

        protected BossPhaseTracker(string debugName)
        {
#if DEBUG
            _debugName = debugName;
#endif
            Instance = (T)this;
        }

        public virtual void ApplyPatches()
        {
            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        public virtual void CleanupPatches()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        void onSceneLoaded(SceneDef _)
        {
            IsInFight = false;
        }
    }
}
