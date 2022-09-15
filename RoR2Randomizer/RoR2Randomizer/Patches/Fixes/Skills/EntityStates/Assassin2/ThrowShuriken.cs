using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.Assassin2
{
    public static class ThrowShuriken
    {
        public static void Apply()
        {
            On.EntityStates.Assassin2.ThrowShuriken.OnEnter += ThrowShuriken_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.Assassin2.ThrowShuriken.OnEnter -= ThrowShuriken_OnEnter;
        }

        static void ThrowShuriken_OnEnter(On.EntityStates.Assassin2.ThrowShuriken.orig_OnEnter orig, global::EntityStates.Assassin2.ThrowShuriken self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, "ShurikenTag");

            orig(self);
        }
    }
}
