#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates.Missions.Arena.NullWard;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.RandomizerControllers.HoldoutZone;
using UnityEngine;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.NullWard
{
    [PatchClass]
    static class NullWardBaseState_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Arena.NullWard.NullWardBaseState.OnEnter += NullWardBaseState_OnEnter;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Arena.NullWard.NullWardBaseState.OnEnter -= NullWardBaseState_OnEnter;
        }

        static void NullWardBaseState_OnEnter(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(NullWardBaseState_NullRefFix)}.{nameof(NullWardBaseState_OnEnter)}";

            ILCursor c = new ILCursor(il);

            if (c.TryFindNext(out ILCursor[] foundCursors,
                              x => x.MatchLdfld<NullWardBaseState>(nameof(NullWardBaseState.sphereZone)),
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(Behaviour), nameof(Behaviour.enabled)))))
            {
                ILCursor iLCursor = foundCursors[foundCursors.Length - 1];
                iLCursor.Emit(OpCodes.Ldarg_0);
                iLCursor.Emit(OpCodes.Call, RandomizedHoldoutZoneController.getZone_MI);
            }
            else
            {
                Log.Warning($"{LOG_PREFIX}: Patch failed");
            }
        }
    }
}
#endif