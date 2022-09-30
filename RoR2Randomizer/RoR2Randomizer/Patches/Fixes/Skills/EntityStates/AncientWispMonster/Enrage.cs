#if !DISABLE_SKILL_RANDOMIZER
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.AncientWispMonster
{
    [PatchClass]
    public static class Enrage
    {
        static void Apply()
        {
            IL.EntityStates.AncientWispMonster.Enrage.FixedUpdate += FixedUpdate_ActivateWithoutAnimatorValues_ILPatch;
        }

        static void Cleanup()
        {
            IL.EntityStates.AncientWispMonster.Enrage.FixedUpdate -= FixedUpdate_ActivateWithoutAnimatorValues_ILPatch;
        }

        static void FixedUpdate_ActivateWithoutAnimatorValues_ILPatch(ILContext il)
        {
            Shared.HookAnimator_GetFloat(il, "Enrage.activate", (float __result, global::EntityStates.AncientWispMonster.Enrage __instance) =>
            {
                if (__result == 0f && __instance.fixedAge >= __instance.duration * 0.75f)
                {
                    return 1f;
                }

                return __result;
            });
        }
    }
}
#endif