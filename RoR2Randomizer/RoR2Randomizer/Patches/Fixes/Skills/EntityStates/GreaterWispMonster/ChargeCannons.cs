using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.GreaterWispMonster
{
    public static class ChargeCannons
    {
        public static void Apply()
        {
            On.EntityStates.GreaterWispMonster.ChargeCannons.OnEnter += ChargeCannons_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.GreaterWispMonster.ChargeCannons.OnEnter -= ChargeCannons_OnEnter;
        }

        static void ChargeCannons_OnEnter(On.EntityStates.GreaterWispMonster.ChargeCannons.orig_OnEnter orig, global::EntityStates.GreaterWispMonster.ChargeCannons self)
        {
            CustomChildTransformManager.AutoAddChildTransforms(self, "MuzzleLeft", "MuzzleRight");
            orig(self);
        }
    }
}
