using EntityStates;
using RoR2Randomizer.BodyAnimationMirroring;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.CharacterAnimationMirroring
{
    [PatchClass]
    static class EntityState_GetModelAnimator_Hook
    {
        internal static int PatchDisabledCount = 0;

        static void Apply()
        {
            On.EntityStates.EntityState.GetModelAnimator += EntityState_GetModelAnimator;
        }

        static void Cleanup()
        {
            On.EntityStates.EntityState.GetModelAnimator -= EntityState_GetModelAnimator;
        }

        static Animator EntityState_GetModelAnimator(On.EntityStates.EntityState.orig_GetModelAnimator orig, EntityState self)
        {
            Animator result = orig(self);
            if (PatchDisabledCount <= 0)
                result = CharacterAnimationMirrorOwner.GetMirrorTargetAnimatorOrOriginal(result);

            return result;
        }
    }
}
