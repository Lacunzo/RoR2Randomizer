using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2Randomizer.BodyAnimationMirroring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.CharacterAnimationMirroring
{
    [PatchClass]
    static class Animator_ReplaceWithMirror_Hook
    {
        internal static int PatchDisabledCount = 0;

        static readonly ILHook[] _animatorILHooks;

        static Animator_ReplaceWithMirror_Hook()
        {
            ILHookConfig config = new ILHookConfig { ManualApply = true };

            MethodInfo[] animatorMethods = typeof(Animator).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.HasMethodBody() && !m.IsGenericMethod).ToArray();
            int animatorCount = animatorMethods.Length;
            _animatorILHooks = new ILHook[animatorCount];
            for (int i = 0; i < animatorCount; i++)
            {
                _animatorILHooks[i] = new ILHook(animatorMethods[i], static il =>
                {
                    ILCursor c = new ILCursor(il);

                    // We do a little trolling
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate(static (UnityEngine.Object instance) =>
                    {
                        if (PatchDisabledCount <= 0 && instance is Animator animator)
                        {
                            return CharacterAnimationMirrorOwner.GetMirrorTargetAnimatorOrOriginal(animator);
                        }
                        else
                        {
                            return instance;
                        }
                    });
                    c.Emit(OpCodes.Starg, 0);
                }, ref config);
            }
        }

        static void Apply()
        {
            if (_animatorILHooks != null)
            {
                foreach (ILHook hook in _animatorILHooks)
                {
                    hook.Apply();
                }
            }
        }

        static void Cleanup()
        {
            if (_animatorILHooks != null)
            {
                foreach (ILHook hook in _animatorILHooks)
                {
                    hook.Undo();
                }
            }
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
