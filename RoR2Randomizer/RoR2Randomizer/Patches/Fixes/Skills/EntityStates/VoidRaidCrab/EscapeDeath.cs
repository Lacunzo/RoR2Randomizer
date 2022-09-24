using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.VoidRaidCrab
{
    public static class EscapeDeath
    {
        public static void Apply()
        {
            On.EntityStates.VoidRaidCrab.EscapeDeath.OnEnter += EscapeDeath_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.VoidRaidCrab.EscapeDeath.OnEnter -= EscapeDeath_OnEnter;
        }

        static void EscapeDeath_OnEnter(On.EntityStates.VoidRaidCrab.EscapeDeath.orig_OnEnter orig, global::EntityStates.VoidRaidCrab.EscapeDeath self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, self.gauntletEntranceChildName);

            orig(self);
        }
    }
}
