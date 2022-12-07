#if !DISABLE_ITEM_RANDOMIZER
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Item;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ItemRandomizer
{
    [PatchClass]
    static class PickupDropletController_PickupRandomizeHook
    {
        static readonly FieldInfo _patchEnabled_FI = AccessTools.DeclaredField(typeof(PickupDropletController_PickupRandomizeHook), nameof(_patchEnabled));

#pragma warning disable IDE0044 // Add readonly modifier
        static bool _patchEnabled = false;
#pragma warning restore IDE0044 // Add readonly modifier

        static readonly MethodInfo PickupDropletController_CreatePickupDroplet_PickupIndex_MI = SymbolExtensions.GetMethodInfo(() => PickupDropletController.CreatePickupDroplet(default(PickupIndex), default, default));

        static readonly MethodInfo PickupDropletController_CreatePickupDroplet_CreatePickupInfo_MI = SymbolExtensions.GetMethodInfo(() => PickupDropletController.CreatePickupDroplet(default(GenericPickupController.CreatePickupInfo), default, default));

        internal static readonly ILContext.Manipulator GenericEnablePatchHook = static il =>
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(x => x.MatchCallOrCallvirt(PickupDropletController_CreatePickupDroplet_PickupIndex_MI)
                                      || x.MatchCallOrCallvirt(PickupDropletController_CreatePickupDroplet_CreatePickupInfo_MI)))
            {
                c.Emit(OpCodes.Ldc_I4_1);
                c.Emit(OpCodes.Stsfld, _patchEnabled_FI);

                c.Index++;

                c.Emit(OpCodes.Ldc_I4_0);
                c.Emit(OpCodes.Stsfld, _patchEnabled_FI);
            }
        };

        static void Apply()
        {
            On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 += PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
        }

        static void Cleanup()
        {
            On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 -= PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;
        }

        static void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3(On.RoR2.PickupDropletController.orig_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 orig, GenericPickupController.CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity)
        {
            if (_patchEnabled)
            {
                if (ItemRandomizerController.TryGetReplacementPickupIndex(pickupInfo.pickupIndex, out PickupIndex replacementPickup))
                {
                    pickupInfo.pickupIndex = replacementPickup;
                }
            }

            orig(pickupInfo, position, velocity);
        }
    }
}
#endif