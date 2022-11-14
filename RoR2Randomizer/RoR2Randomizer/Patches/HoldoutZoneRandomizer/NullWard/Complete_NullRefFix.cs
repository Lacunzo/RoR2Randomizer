#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.HoldoutZone;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.NullWard
{
    [PatchClass]
    static class Complete_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Arena.NullWard.Complete.OnEnter += setNetworkRadiusPatch;
            IL.EntityStates.Missions.Arena.NullWard.Complete.FixedUpdate += setNetworkRadiusPatch;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Arena.NullWard.Complete.OnEnter -= setNetworkRadiusPatch;
            IL.EntityStates.Missions.Arena.NullWard.Complete.FixedUpdate -= setNetworkRadiusPatch;
        }

        static void setNetworkRadiusPatch(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(Active_NullRefFix)}.{nameof(setNetworkRadiusPatch)}";

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
        }
    }
}
#endif