using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Buff;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    [PatchClass]
    public static class BuffIndexPatch
    {
        public static uint SkipPatchCount = 0;

        static ILHook[] CharacterBody_AddTimedBuff_BuffDef_float_localFunctionPatches;

        static float? _currentAddTimedBuffDuration;

        static void Apply()
        {
            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;

            IL.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += IL_CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += On_CharacterBody_AddTimedBuff_BuffDef_float;

            IL.RoR2.CharacterBody.AddBuff_BuffIndex += replaceReadBuffCountFromArray;
            IL.RoR2.CharacterBody.RemoveBuff_BuffIndex += replaceReadBuffCountFromArray;
        }

        static void On_CharacterBody_AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            _currentAddTimedBuffDuration = duration;
            orig(self, buffDef, duration);
            _currentAddTimedBuffDuration = null;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterBody.SetBuffCount -= CharacterBody_SetBuffCount;

            IL.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= IL_CharacterBody_AddTimedBuff_BuffDef_float;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= On_CharacterBody_AddTimedBuff_BuffDef_float;

            IL.RoR2.CharacterBody.AddBuff_BuffIndex -= replaceReadBuffCountFromArray;
            IL.RoR2.CharacterBody.RemoveBuff_BuffIndex -= replaceReadBuffCountFromArray;

            if (CharacterBody_AddTimedBuff_BuffDef_float_localFunctionPatches != null)
            {
                foreach (ILHook hook in CharacterBody_AddTimedBuff_BuffDef_float_localFunctionPatches)
                {
                    hook?.Undo();
                }
            }
        }

        static void IL_CharacterBody_AddTimedBuff_BuffDef_float(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(BuffIndexPatch)}.{nameof(IL_CharacterBody_AddTimedBuff_BuffDef_float)} ";

            static BuffIndex simpleReplaceBuffIndex(BuffIndex original)
            {
                if (SkipPatchCount == 0 && BuffRandomizerController.IsActive)
                {
                    BuffRandomizerController.TryReplaceBuffIndex(ref original);
                }

                return original;
            }

            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(x => x.MatchStfld<CharacterBody.TimedBuff>(nameof(CharacterBody.TimedBuff.buffIndex))))
            {
                c.EmitDelegate(simpleReplaceBuffIndex);

                c.Index++;
            }

            c.Index = 0;
            while (c.TryGotoNext(x => x.MatchLdfld<CharacterBody.TimedBuff>(nameof(CharacterBody.TimedBuff.buffIndex))))
            {
                c.Index++;
                c.EmitDelegate(simpleReplaceBuffIndex);
            }

            HashSet<MethodInfo> localFunctionsToHook = new HashSet<MethodInfo>();
            c.Index = 0;
            while (c.TryGotoNext(x => x.MatchCallOrCallvirt(out MethodReference method) && method.Name.StartsWith($"<{nameof(CharacterBody.AddTimedBuff)}>g__")))
            {
                MethodReference methodRef = (MethodReference)c.Next.Operand;
                MethodInfo methodInfo = typeof(CharacterBody).GetMethod(methodRef.Name, BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                if (methodInfo != null)
                {
                    localFunctionsToHook.Add(methodInfo);
                }
                else
                {
                    Log.Warning(LOG_PREFIX + $"unable to find method {methodRef.Name}");
                }
            }

            if (CharacterBody_AddTimedBuff_BuffDef_float_localFunctionPatches == null)
            {
                CharacterBody_AddTimedBuff_BuffDef_float_localFunctionPatches = new ILHook[localFunctionsToHook.Count];
                using (HashSet<MethodInfo>.Enumerator enumerator = localFunctionsToHook.GetEnumerator())
                {
                    ILHookConfig hookConfig = new ILHookConfig { ManualApply = false };
                    for (int i = 0; enumerator.MoveNext(); i++)
                    {
                        CharacterBody_AddTimedBuff_BuffDef_float_localFunctionPatches[i] = new ILHook(enumerator.Current, static il =>
                        {
                            ILCursor c = new ILCursor(il);

                            while (c.TryGotoNext(x => x.MatchStfld<CharacterBody.TimedBuff>(nameof(CharacterBody.TimedBuff.buffIndex))))
                            {
                                c.EmitDelegate(simpleReplaceBuffIndex);

                                c.Index++;
                            }

                            c.Index = 0;
                            while (c.TryGotoNext(x => x.MatchLdfld<CharacterBody.TimedBuff>(nameof(CharacterBody.TimedBuff.buffIndex))))
                            {
                                c.Index++;
                                c.EmitDelegate(simpleReplaceBuffIndex);
                            }
                        }, hookConfig);
                    }
                }
            }
        }

        static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            if (SkipPatchCount == 0 &&
                BuffRandomizerController.IsActive &&
                BuffRandomizerController.TryReplaceBuffIndex(ref buffType))
            {
                if (BuffRandomizerController.TryGetDotIndex(buffType, out DotController.DotIndex dot))
                {
                    int diff = newCount - self.buffs[(int)buffType];

                    // Only apply DOT if buff stack is increasing
                    if (diff > 0)
                    {
#if DEBUG
                        Log.Debug($"Buff randomizer: Applying dot {dot}");
#endif

                        GameObject attacker = self.gameObject;

                        HealthComponent healthComponent = self.healthComponent;
                        if (healthComponent)
                        {
                            GameObject lastAttacker = healthComponent.lastHitAttacker;
                            if (lastAttacker)
                            {
                                attacker = lastAttacker;
                            }
                        }

                        DotRandomizerPatch.SkipApplyBuffCount++;

                        float duration = _currentAddTimedBuffDuration ?? float.PositiveInfinity;

#if DEBUG
                        Log.Debug($"Applying randomized buff->DOT {dot}x{diff} for {duration} seconds");
#endif

                        for (int i = 0; i < diff; i++)
                        {
                            DotController.InflictDot(self.gameObject, attacker, dot, duration);
                        }

                        DotRandomizerPatch.SkipApplyBuffCount--;

                        return;
                    }
                    else if (diff < 0)
                    {
                        // Buff stack is decreasing, do stacks should be removed

                        DotController dotController = DotController.FindDotController(self.gameObject);
                        if (dotController)
                        {
                            int foundStacks = 0;

                            List<DotController.DotStack> stacks = dotController.dotStackList;
                            for (int i = stacks.Count - 1; i >= 0 && foundStacks < -diff; i--)
                            {
                                DotController.DotStack dotStack = stacks[i];
                                if (dotStack == null || dotStack.dotIndex != dot)
                                    continue;

                                DotController.DotDef dotDef = dotStack.dotDef;
                                if (dotDef == null)
                                    continue;

                                BuffDef associatedBuff = dotDef.associatedBuff;
                                if (!associatedBuff)
                                    continue;

                                if (associatedBuff.buffIndex == buffType)
                                {
                                    foundStacks++;
                                    SkipPatchCount++;
                                    dotController.RemoveDotStackAtServer(i);
                                    SkipPatchCount--;
                                }
                            }

                            if (foundStacks == -diff)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            orig(self, buffType, newCount);
        }

        static void replaceReadBuffCountFromArray(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryFindNext(out ILCursor[] cursors, x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.buffs)), 
                                                                x => x.MatchLdelemI4()))
            {
                ILCursor last = cursors[cursors.Length - 1];
                last.EmitDelegate((int buffIndex) =>
                {
                    if (SkipPatchCount == 0 && BuffRandomizerController.IsActive)
                    {
                        BuffRandomizerController.TryReplaceBuffIndex(ref buffIndex);
                    }

                    return buffIndex;
                });

                // Make sure it does not match the same instructions again
                c.Index = last.Index + 1;
            }
        }
    }
}
