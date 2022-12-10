using MonoMod.Cil;
using RoR2.Orbs;

namespace RoR2Randomizer.Patches.ProjectileRandomizer.Orbs
{
    [PatchClass]
    // Make LightningOrbs not re-randomize in OnArrival
    static class LightningOrb_PreventSpawnHook
    {
        static void Apply()
        {
            IL.RoR2.Orbs.LightningOrb.OnArrival += LightningOrb_OnArrival;
        }

        static void Cleanup()
        {
            IL.RoR2.Orbs.LightningOrb.OnArrival -= LightningOrb_OnArrival;
        }

        static void LightningOrb_OnArrival(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(LightningOrb_PreventSpawnHook)}.{nameof(LightningOrb_OnArrival)} ";

            ILCursor c = new ILCursor(il);

            int numPatchesMade = 0;

            while (c.TryGotoNext(x => x.MatchCallOrCallvirt<OrbManager>(nameof(OrbManager.AddOrb))))
            {
                c.EmitDelegate(static () =>
                {
                    OrbManager_AddOrbHook.PatchDisabledCount++;
                });

                c.Index++;

                c.EmitDelegate(static () =>
                {
                    OrbManager_AddOrbHook.PatchDisabledCount--;
                });

                numPatchesMade++;
            }

            if (numPatchesMade == 0)
            {
                Log.Warning(LOG_PREFIX + "found no patch locations");
            }
#if DEBUG
            else
            {
                Log.Debug(LOG_PREFIX + $"found {numPatchesMade} patch locations");
            }
#endif
        }
    }
}
