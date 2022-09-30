#if !DISABLE_SKILL_RANDOMIZER
using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.Drone.DroneWeapon
{
    [PatchClass]
    public static class FireTurret
    {
        static void Apply()
        {
            On.EntityStates.Drone.DroneWeapon.FireTurret.OnEnter += FireTurret_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.Drone.DroneWeapon.FireTurret.OnEnter -= FireTurret_OnEnter;
        }

        static void FireTurret_OnEnter(On.EntityStates.Drone.DroneWeapon.FireTurret.orig_OnEnter orig, global::EntityStates.Drone.DroneWeapon.FireTurret self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, "Muzzle");
            orig(self);
        }
    }
}
#endif