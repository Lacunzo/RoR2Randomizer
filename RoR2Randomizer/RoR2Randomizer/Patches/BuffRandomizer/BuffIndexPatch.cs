using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Buff;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.Patches.BuffRandomizer
{
    [PatchClass]
    public static class BuffIndexPatch
    {
        public static uint SkipApplyDotCount = 0;

        static void Apply()
        {
            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;

            IL.RoR2.CharacterBody.AddBuff_BuffIndex += replaceReadBuffCountFromArray;
            IL.RoR2.CharacterBody.RemoveBuff_BuffIndex += replaceReadBuffCountFromArray;

#if DEBUG
            On.RoR2.CharacterBody.RemoveBuff_BuffDef += CharacterBody_RemoveBuff_BuffDef;
#endif
        }

        static void Cleanup()
        {
            On.RoR2.CharacterBody.SetBuffCount -= CharacterBody_SetBuffCount;

            IL.RoR2.CharacterBody.AddBuff_BuffIndex -= replaceReadBuffCountFromArray;
            IL.RoR2.CharacterBody.RemoveBuff_BuffIndex -= replaceReadBuffCountFromArray;

#if DEBUG
            On.RoR2.CharacterBody.RemoveBuff_BuffDef -= CharacterBody_RemoveBuff_BuffDef;
#endif
        }

#if DEBUG
        static void CharacterBody_RemoveBuff_BuffDef(On.RoR2.CharacterBody.orig_RemoveBuff_BuffDef orig, CharacterBody self, BuffDef buffDef)
        {
            Log.LogType("CharacterBody_RemoveBuff_BuffDef on client stacktrace: " + new System.Diagnostics.StackTrace().ToString(), BepInEx.Logging.LogLevel.Debug);
            orig(self, buffDef);
        }
#endif

        static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            if (BuffRandomizerController.IsActive &&
                SkipApplyDotCount == 0 &&
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

                        GameObject attacker = null;

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
                        DotController.InflictDot(self.gameObject, attacker ?? self.gameObject, dot);
                        DotRandomizerPatch.SkipApplyBuffCount--;

                        return;
                    }
                    else if (diff < 0)
                    {
                        DotController dotController = DotController.FindDotController(self.gameObject);
                        if (dotController)
                        {
                            int foundStacks = 0;

                            List<DotController.DotStack> stacks = dotController.dotStackList;
                            for (int i = stacks.Count - 1; i >= 0 && foundStacks < -diff; i--)
                            {
                                DotController.DotStack dotStack = stacks[i];
                                if (dotStack != null && dotStack.dotIndex == dot)
                                {
                                    DotController.DotDef dotDef = dotStack.dotDef;
                                    if (dotDef != null)
                                    {
                                        BuffDef associatedBuff = dotDef.associatedBuff;
                                        if (associatedBuff)
                                        {
                                            if (associatedBuff.buffIndex == buffType)
                                            {
                                                foundStacks++;
                                                SkipApplyDotCount++;
                                                dotController.RemoveDotStackAtServer(i);
                                                SkipApplyDotCount--;
                                            }
                                        }
                                    }
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
                    if (BuffRandomizerController.IsActive && SkipApplyDotCount == 0)
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
