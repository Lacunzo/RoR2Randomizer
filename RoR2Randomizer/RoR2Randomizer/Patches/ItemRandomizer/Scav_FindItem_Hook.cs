#if !DISABLE_ITEM_RANDOMIZER
using EntityStates.ScavMonster;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Item;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class Scav_FindItem_Hook
    {
        static void Apply()
        {
            IL.EntityStates.ScavMonster.FindItem.OnEnter += FindItem_OnEnter;
        }

        static void Cleanup()
        {
            IL.EntityStates.ScavMonster.FindItem.OnEnter -= FindItem_OnEnter;
        }

        static void FindItem_OnEnter(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdarg(0),
                              x => x.MatchLdfld<FindItem>(nameof(FindItem.dropPickup)),
                              x => x.MatchCallOrCallvirt<PickupDisplay>(nameof(PickupDisplay.SetPickupIndex))))
            {
                ILCursor dropPickupCursor = foundCursors[1];
                dropPickupCursor.Index++;
                dropPickupCursor.Emit(OpCodes.Ldarg_0);
                dropPickupCursor.EmitDelegate(static (PickupIndex dropPickup, FindItem instance) =>
                {
                    if (instance != null && instance.isAuthority)
                    {
                        return instance.dropPickup = ItemRandomizerController.GetReplacementPickupIndex(dropPickup);
                    }

                    return dropPickup;
                });
            }
        }
    }
}
#endif