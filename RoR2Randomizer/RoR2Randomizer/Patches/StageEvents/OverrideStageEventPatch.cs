using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.StageEvents
{
    [PatchClass]
    static class OverrideStageEventPatch
    {
        internal static DirectorCardCategorySelection ForcedCategorySelection;

        static void Apply()
        {
            IL.RoR2.ClassicStageInfo.RebuildCards += ClassicStageInfo_RebuildCards;
        }

        static void Cleanup()
        {
            IL.RoR2.ClassicStageInfo.RebuildCards -= ClassicStageInfo_RebuildCards;
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

            c.Index = 0;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<ClassicStageInfo>(nameof(ClassicStageInfo.monsterCategories)),
                              x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo(() => GameObject.Instantiate<DirectorCardCategorySelection>(default)))))
            {
                foundCursors[1].EmitDelegate(getCardSelectionPrefab);
            }
        }
    }
}
