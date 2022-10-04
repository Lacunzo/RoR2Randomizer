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

        protected BossTracker()
        {
            Instance = (T)this;
        }

        public TrackedValue<bool> IsInFight = new TrackedValue<bool>();

        protected virtual void applyPatches()
        {
            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        protected virtual void cleanupPatches()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        // Virtual methods don't work property in events, so this wrapper method is needed to make it work
        void onSceneLoaded_Wrap(SceneDef scene)
        {
            onSceneLoaded(scene);
        }

        protected virtual void onSceneLoaded(SceneDef scene)
        {
            IsInFight.Value = false;
        }
    }
}
