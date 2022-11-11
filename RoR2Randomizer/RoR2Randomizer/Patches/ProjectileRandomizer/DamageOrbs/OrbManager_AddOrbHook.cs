using RoR2.Orbs;
using RoR2Randomizer.RandomizerControllers.Projectile;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ProjectileRandomizer.DamageOrbs
{
    [PatchClass]
    static class OrbManager_AddOrbHook
    {
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
            if (ProjectileRandomizerController.TryReplaceFire(orb))
            {
                return;
            }

            orig(self, orb);
        }
    }
}
