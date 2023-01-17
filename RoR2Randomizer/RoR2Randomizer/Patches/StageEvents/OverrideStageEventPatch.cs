using HarmonyLib;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RoR2Randomizer.Patches.StageEvents
{
    [PatchClass]
    static class OverrideStageEventPatch
    {
        internal static DirectorCardCategorySelection ForcedCategorySelection;

        static Hook[] canFamilyEventTriggerHooks;

        static void Apply()
        {
            IL.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;

            canFamilyEventTriggerHooks = (from type in typeof(Run).Assembly.GetTypes()
                                          where typeof(Run).IsAssignableFrom(type)
                                          let property = type.GetProperty("canFamilyEventTrigger", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                          where property != null
                                          select new Hook(property.GetMethod, Run_get_canFamilyEventTrigger)).ToArray();
        }

        static void Cleanup()
        {
            IL.RoR2.ClassicStageInfo.RebuildCards -= ClassicStageInfo_RebuildCards;

            if (canFamilyEventTriggerHooks != null)
            {
                foreach (Hook hook in canFamilyEventTriggerHooks)
                {
                    hook?.Undo();
                }
            }
        }

        static bool Run_get_canFamilyEventTrigger(Func<Run, bool> orig, Run self)
        {
            return ForcedCategorySelection || orig(self);
        }

        static void ClassicStageInfo_RebuildCards(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            static DirectorCardCategorySelection getCardSelectionPrefab(DirectorCardCategorySelection selected)
            {
                return ForcedCategorySelection ? ForcedCategorySelection : selected;
            }

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<ClassicStageInfo>(nameof(ClassicStageInfo.monsterDccsPool)),
                              x => x.MatchCallOrCallvirt<DccsPool>(nameof(DccsPool.GenerateWeightedSelection)),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<WeightedSelection<DirectorCardCategorySelection>>(_ => _.Evaluate(default)))))
            {
                ILCursor ilCursor = foundCursors[2];
                ilCursor.Index++;

                ilCursor.EmitDelegate(getCardSelectionPrefab);
            }
            else
            {
                Log.Warning("unable to find patch location (0)");
            }

            c.Index = 0;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<ClassicStageInfo>(nameof(ClassicStageInfo.monsterCategories)),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => GameObject.Instantiate<DirectorCardCategorySelection>(default)))))
            {
                foundCursors[1].EmitDelegate(getCardSelectionPrefab);
            }
            else
            {
                Log.Warning("unable to find patch location (1)");
            }
        }
    }
}
