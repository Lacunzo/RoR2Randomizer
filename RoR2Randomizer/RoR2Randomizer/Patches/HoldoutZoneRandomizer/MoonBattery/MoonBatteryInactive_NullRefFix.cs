#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using EntityStates.Missions.Moon;
using HarmonyLib;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Patches.Fixes;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.MoonBattery
{
    [PatchClass]
    static class MoonBatteryInactive_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnEnter += InactiveFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnEnter += purchaseInteraction_NullRefFix;

            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnExit += InactiveFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnExit += purchaseInteraction_NullRefFix;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnEnter -= InactiveFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnEnter -= purchaseInteraction_NullRefFix;

            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnExit -= InactiveFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryInactive.OnExit -= purchaseInteraction_NullRefFix;
        }

        static void purchaseInteraction_NullRefFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            while (c.TryFindNext(out foundCursors,
                                 x => x.MatchLdfld<MoonBatteryBaseState>(nameof(MoonBatteryBaseState.purchaseInteraction)),
                                 x => x.MatchCallOrCallvirt<PurchaseInteraction>(nameof(PurchaseInteraction.SetAvailable))))
            {
                foundCursors[1].ReplaceCall((PurchaseInteraction instance, bool available) =>
                {
                    if (instance)
                    {
                        instance.SetAvailable(available);
                    }
                });

                c.Index = foundCursors[foundCursors.Length - 1].Index + 1;
            }
        }

        static void InactiveFX_NullRefFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            while (c.TryFindNext(out foundCursors,
                                 x => x.MatchLdstr("InactiveFX"),
                                 x => x.MatchCallOrCallvirt<BaseState>(nameof(BaseState.FindModelChild)),
                                 x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(Component), nameof(Component.gameObject))),
                                 x => x.MatchCallOrCallvirt<GameObject>(nameof(GameObject.SetActive))))
            {
                Shared.Try_get_gameObject(foundCursors[2]);

                foundCursors[3].ReplaceCall((GameObject obj, bool active) =>
                {
                    if (obj)
                    {
                        obj.SetActive(active);
                    }
                });

                c.Index = foundCursors[foundCursors.Length - 1].Index + 1;
            }
        }
    }
}
#endif