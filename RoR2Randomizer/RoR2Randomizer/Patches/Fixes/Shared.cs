using EntityStates;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using RoR2Randomizer.Utility.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes
{
    public static class Shared
    {
        // Fixes an incorrect Physics.Raycast call which passes the layermask as the maxDistance:
        // Before:
        // Physics.Raycast(ray, out raycastHit, (float)layerMask)
        // After:
        // Physicx.Raycast(ray, out raycastHit, float.PositiveInfinity, layerMask)
        public static void Physics_Raycast_LayerMaskDistanceFix_ILPatch(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchImplicitConversion<LayerMask, int>(),
                                 x => x.MatchConvR4(),
                                 x => x.MatchCall(StaticReflectionCache.Physics_Raycast_Ray_outRaycastHit_float_MI)))
            {
                c.Index++; // Skip to after implicit conversion call

                // Remove Conv_R4
                // Remove incorrect Physics.Raycast call
                c.RemoveRange(2);

                c.EmitDelegate((Ray ray, out RaycastHit hit, int layerMask) =>
                {
                    return Physics.Raycast(ray, out hit, float.PositiveInfinity, layerMask);
                });
            }
        }

        public static void HookAnimator_GetFloat<T>(ILContext il, string name, T hook) where T : Delegate
        {
            HookAnimator_GetValue(il, name, StaticReflectionCache.Animator_GetFloat_name_MI, hook);
        }

        public static void HookAnimator_GetValue<T>(ILContext il, string name, MethodInfo targetMethod, T hook) where T : Delegate
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After, x => x.MatchLdstr(name), x => x.MatchCallvirt(targetMethod)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(hook);
            }
        }

        static readonly string[] _skillFieldNames = new string[] { nameof(SkillLocator.primary), nameof(SkillLocator.secondary), nameof(SkillLocator.utility), nameof(SkillLocator.special) };
        public static void EntityState_SkillSlotResolver(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            while (c.TryGotoNext(x => x.MatchLdarg(0),
                                 x => x.MatchCall(StaticReflectionCache.EntityState_get_skillLocator),
                                 x => x.MatchLdfld(out FieldReference field) && Array.IndexOf(_skillFieldNames, field.Name) != -1))
            {
                c.Index += 2; // Skip to ldfld instruction
                c.Remove(); // Remove ldfld

                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate((SkillLocator locator, EntityState entityState) =>
                {
                    GenericSkill owner = EntityStateOwnerTracker.GetOwner(entityState);
                    if (owner)
                    {
                        return owner;
                    }
                    else if (locator)
                    {
                        Log.Warning("EntityState_SkillSlotResolver: No state owner found");
                        return locator.primary;
                    }
                    else
                    {
                        throw new NullReferenceException("SkillLocator is null");
                    }
                });
            }
        }

        public static void TryAddTemporaryComponentIfMissing<T>(CharacterBody body, ref T cachedComponent) where T : MonoBehaviour
        {
            if (body && !cachedComponent)
            {
                cachedComponent = TempComponentsTracker.AddTempComponent<T>(body);
            }
        }

        public static void Try_get_gameObject(ILCursor c)
        {
            ReplaceCall(c, (Component comp) =>
            {
                return comp ? comp.gameObject : null;
            });
        }

        public static void ReplaceCall<T>(this ILCursor c, T del) where T : Delegate
        {
            c.Remove();
            c.EmitDelegate(del);
        }
    }
}
