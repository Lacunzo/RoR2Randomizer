#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer
{
    [PatchClass]
    static class HoldoutZoneRadiusSync
    {
        static void Apply()
        {
            IL.RoR2.HoldoutZoneController.FixedUpdate += HoldoutZoneController_FixedUpdate;
        }

        static void Cleanup()
        {
            IL.RoR2.HoldoutZoneController.FixedUpdate -= HoldoutZoneController_FixedUpdate;
        }

        static void HoldoutZoneController_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdloca(out _),
                              x => x.MatchCallOrCallvirt<HoldoutZoneController.CalcRadiusDelegate>(nameof(HoldoutZoneController.CalcRadiusDelegate.Invoke))))
            {
                foundCursors[0].Next.MatchLdloca(out int localIndex);

                c.Index = foundCursors[foundCursors.Length - 1].Index + 1;

                if (c.TryGotoNext(x => x.MatchLdloc(localIndex)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldloc, localIndex);
                    c.EmitDelegate((HoldoutZoneController instance, float calculatedRadius) =>
                    {
                        if (instance && instance.TryGetComponent<RandomizedHoldoutZoneController>(out RandomizedHoldoutZoneController randomizedZoneController))
                        {
                            randomizedZoneController.TrySetZoneRadius(calculatedRadius);
                        }
                    });
                }
            }
        }
    }
}
#endif