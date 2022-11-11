using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class DirectorCore_TrySpawnObject
    {
        public delegate void PostfixDelegate(ref GameObject result, DirectorSpawnRequest directorSpawnRequest);
        public static event PostfixDelegate Postfix;

        static void Apply()
        {
            On.RoR2.DirectorCore.TrySpawnObject += DirectorCore_TrySpawnObject_EventsHook;
        }

        static void Cleanup()
        {
            On.RoR2.DirectorCore.TrySpawnObject -= DirectorCore_TrySpawnObject_EventsHook;
        }

        static GameObject DirectorCore_TrySpawnObject_EventsHook(On.RoR2.DirectorCore.orig_TrySpawnObject orig, DirectorCore self, DirectorSpawnRequest directorSpawnRequest)
        {
            GameObject result = orig(self, directorSpawnRequest);
            Postfix?.Invoke(ref result, directorSpawnRequest);
            return result;
        }
    }
}
