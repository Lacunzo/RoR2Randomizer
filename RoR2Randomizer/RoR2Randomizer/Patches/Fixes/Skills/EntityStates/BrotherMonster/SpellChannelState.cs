using RoR2Randomizer.ChildTransformAdditions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.BrotherMonster
{
    [PatchClass]
    public static class SpellChannelState
    {
        static void Apply()
        {
            On.EntityStates.BrotherMonster.SpellChannelState.OnEnter += SpellChannelState_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.BrotherMonster.SpellChannelState.OnEnter -= SpellChannelState_OnEnter;
        }

        static void SpellChannelState_OnEnter(On.EntityStates.BrotherMonster.SpellChannelState.orig_OnEnter orig, global::EntityStates.BrotherMonster.SpellChannelState self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, "SpellChannel");

            orig(self);
        }
    }
}
