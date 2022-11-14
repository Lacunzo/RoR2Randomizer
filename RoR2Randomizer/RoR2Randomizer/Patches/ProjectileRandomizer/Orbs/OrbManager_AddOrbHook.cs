using RoR2.Orbs;
using RoR2Randomizer.RandomizerControllers.Projectile;

namespace RoR2Randomizer.Patches.ProjectileRandomizer.Orbs
{
    [PatchClass]
    static class OrbManager_AddOrbHook
    {
        internal static int PatchDisabledCount = 0;

        static void Apply()
        {
            On.RoR2.Orbs.OrbManager.AddOrb += OrbManager_AddOrb;
        }

        static void Cleanup()
        {
            On.RoR2.Orbs.OrbManager.AddOrb -= OrbManager_AddOrb;
        }

        static void OrbManager_AddOrb(On.RoR2.Orbs.OrbManager.orig_AddOrb orig, OrbManager self, Orb orb)
        {
            if (PatchDisabledCount <= 0 && ProjectileRandomizerController.TryReplaceFire(orb))
            {
                return;
            }

            orig(self, orb);
        }
    }
}
