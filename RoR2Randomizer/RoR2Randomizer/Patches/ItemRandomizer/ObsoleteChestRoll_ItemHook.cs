#if !DISABLE_ITEM_RANDOMIZER
using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Item;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class ObsoleteChestRoll_ItemHook
    {
        static void Apply()
        {
            IL.RoR2.ChestBehavior.PickFromList += ChestBehavior_PickFromList;
        }

        static void Cleanup()
        {
            IL.RoR2.ChestBehavior.PickFromList -= ChestBehavior_PickFromList;
        }

        static void ChestBehavior_PickFromList(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertySetter(typeof(ChestBehavior), nameof(ChestBehavior.dropPickup)))))
            {
                c.EmitDelegate(ItemRandomizerController.GetReplacementPickupIndex);
                c.Index++;
            }
        }
    }
}
#endif