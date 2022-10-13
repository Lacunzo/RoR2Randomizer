#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Patches.Fixes;
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.NullWard
{
    [PatchClass]
    static class Active_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Arena.NullWard.Active.FixedUpdate += Active_FixedUpdate;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Arena.NullWard.Active.FixedUpdate -= Active_FixedUpdate;
        }

        static void Active_FixedUpdate(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(Active_NullRefFix)}.{nameof(Active_FixedUpdate)}";

            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertySetter(typeof(SphereZone), nameof(SphereZone.Networkradius)))))
            {
                c.Emit(OpCodes.Ldarg_0);

                c.Remove();
                c.Emit(OpCodes.Call, RandomizedHoldoutZoneController.SetSphereZoneRadius_MI);
            }
            else
            {
                Log.Warning($"{LOG_PREFIX}: Patch failed");
            }

            Log.Debug(il.ToString());
        }
    }
}
#endif