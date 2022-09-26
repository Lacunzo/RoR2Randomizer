using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.VoidRaidCrab
{
    public static class DeathState
    {
        public static void Apply()
        {
            On.EntityStates.VoidRaidCrab.DeathState.OnEnter += DeathState_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.VoidRaidCrab.DeathState.OnEnter -= DeathState_OnEnter;
        }

        static void DeathState_OnEnter(On.EntityStates.VoidRaidCrab.DeathState.orig_OnEnter orig, global::EntityStates.VoidRaidCrab.DeathState self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, self.initialEffectMuzzle);
            orig(self);
        }
    }
}
