#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer
{
    [PatchClass]
    static class HoldoutZoneControllerInitHook
    {
        static void Apply()
        {
            On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;
        }

        static void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, RoR2.HoldoutZoneController self)
        {
            HoldoutZoneRandomizerController.TryRandomizeHoldoutZone(self);
            orig(self);
        }
    }
}
#endif