#if !DISABLE_ITEM_RANDOMIZER
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerControllers.Item;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Patches.ItemRandomizer.ArtifactKey
{
    [PatchClass]
    static class ReplaceArtifactKeyPatch
    {
        static void Apply()
        {
            IL.RoR2.ArtifactTrialMissionController.SetupState.FixedUpdate += hookFindPickupIndex_ItemIndex;

            IL.RoR2.ArtifactTrialMissionController.RemoveAllMissionKeys += hookFindPickupIndex_ItemIndex;
            IL.RoR2.ArtifactTrialMissionController.RemoveAllMissionKeys += ArtifactTrialMissionController_RemoveAllMissionKeys;
        }

        static void Cleanup()
        {
            IL.RoR2.ArtifactTrialMissionController.SetupState.FixedUpdate -= hookFindPickupIndex_ItemIndex;

            IL.RoR2.ArtifactTrialMissionController.RemoveAllMissionKeys -= hookFindPickupIndex_ItemIndex;
            IL.RoR2.ArtifactTrialMissionController.RemoveAllMissionKeys -= ArtifactTrialMissionController_RemoveAllMissionKeys;
        }

        static void hookFindPickupIndex_ItemIndex(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After, matchCallFindPickupIndex_ItemIndex))
            {
                c.EmitDelegate(ItemRandomizerController.GetReplacementPickupIndex);
            }
        }

        static void ArtifactTrialMissionController_RemoveAllMissionKeys(ILContext il)
        {
            const string LOG_PREFIX = $"{nameof(ReplaceArtifactKeyPatch)}.{nameof(ArtifactTrialMissionController_RemoveAllMissionKeys)} ";

            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            int pickupIndexLocalIndex = -1;
            if (c.TryFindNext(out foundCursors, matchCallFindPickupIndex_ItemIndex, x => x.MatchStloc(out pickupIndexLocalIndex)))
            {
                ILLabel moveNextLbl = null;
                int characterMasterLocalIndex = -1;
                if (c.TryFindNext(out foundCursors,
                                  x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(CharacterMaster), nameof(CharacterMaster.readOnlyInstancesList))),
                                  x => x.MatchCallOrCallvirt(SymbolExtensions.GetMethodInfo<ReadOnlyCollection<CharacterMaster>>(_ => _.GetEnumerator())),
                                  x => x.MatchBr(out moveNextLbl),
                                  x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(IEnumerator<CharacterMaster>), nameof(IEnumerator<CharacterMaster>.Current))),
                                  x => x.MatchStloc(out characterMasterLocalIndex)))
                {
                    ILCursor foreachStart = foundCursors[4];
                    foreachStart.Index++;

                    foreachStart.Emit(OpCodes.Ldloc, characterMasterLocalIndex);
                    foreachStart.Emit(OpCodes.Ldloc, pickupIndexLocalIndex);
                    foreachStart.EmitDelegate(static (CharacterMaster characterMaster, PickupIndex pickupIndex) =>
                    {
                        PickupDef pickupDef = pickupIndex.pickupDef;
                        if (pickupDef != null)
                        {
                            if (pickupDef.itemIndex != ItemIndex.None)
                            {
                                if (pickupDef.miscPickupIndex == MiscPickupIndex.None) // Don't remove all coins
                                {
                                    pickupDef.TryDeductFrom(characterMaster, int.MaxValue);
                                    return true;
                                }
                            }
                        }

                        return false;
                    });
                    foreachStart.Emit(OpCodes.Brtrue, moveNextLbl);
                }
                else
                {
                    Log.Warning(LOG_PREFIX + "unable to find CharacterMaster.readOnlyInstancesList foreach");
                }
            }
            else
            {
                Log.Warning(LOG_PREFIX + "unable to find pickup index local index");
            }
        }

        static readonly MethodInfo PickupCatalog_FindPickupIndex_ItemIndex_MI = SymbolExtensions.GetMethodInfo(() => PickupCatalog.FindPickupIndex(default(ItemIndex)));
        static bool matchCallFindPickupIndex_ItemIndex(Instruction instr)
        {
            return instr.MatchCallOrCallvirt(PickupCatalog_FindPickupIndex_ItemIndex_MI);
        }
    }
}
#endif