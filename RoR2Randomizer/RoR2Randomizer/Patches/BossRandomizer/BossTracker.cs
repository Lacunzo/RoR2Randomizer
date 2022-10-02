using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer
{
    public class BossTracker<T> where T : BossTracker<T>
    {
        public static T Instance { get; private set; }

#if DEBUG
        protected readonly string _debugName;
#endif

        protected BossTracker(string debugName)
        {
#if DEBUG
            _debugName = debugName;
#endif
            Instance = (T)this;
        }

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

        protected virtual void applyPatches()
        {
            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        protected virtual void cleanupPatches()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        void onSceneLoaded(SceneDef _)
        {
            IsInFight = false;
        }
    }
}
