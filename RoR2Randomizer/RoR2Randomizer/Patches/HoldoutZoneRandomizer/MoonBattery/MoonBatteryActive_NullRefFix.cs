#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.UI;
using RoR2Randomizer.Patches.Fixes;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.MoonBattery
{
    [PatchClass]
    static class MoonBatteryActive_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter += ChargingFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter += PositionIndicatorPosition_NullRefFix;

            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnExit += ChargingFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnExit += PositionIndicatorPosition_NullRefFix;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter -= ChargingFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnEnter -= PositionIndicatorPosition_NullRefFix;

            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnExit -= ChargingFX_NullRefFix;
            IL.EntityStates.Missions.Moon.MoonBatteryActive.OnExit -= PositionIndicatorPosition_NullRefFix;
        }

        static void ChargingFX_NullRefFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            while (c.TryFindNext(out foundCursors,
                                 x => x.MatchLdstr("ChargingFX"),
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

        static void PositionIndicatorPosition_NullRefFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdstr("PositionIndicatorPosition"),
                              x => x.MatchCallOrCallvirt<BaseState>(nameof(BaseState.FindModelChild))))
            {
                ILLabel afterPositionIndicator = il.DefineLabel();

                ILCursor findModelChild = foundCursors[1];
                findModelChild.Index++;
                findModelChild.Emit(OpCodes.Dup);
                findModelChild.Emit(OpCodes.Call, ReflectionUtils.FindImplicitConversion(typeof(UnityEngine.Object), typeof(bool)));
                findModelChild.Emit(OpCodes.Brfalse, afterPositionIndicator);

                if (findModelChild.TryGotoNext(MoveType.After,
                                               x => x.MatchStfld<ChargeIndicatorController>(nameof(ChargeIndicatorController.holdoutZoneController))))
                {
                    ILLabel retLabel = il.DefineLabel();
                    findModelChild.Emit(OpCodes.Br, retLabel);

                    findModelChild.Emit(OpCodes.Pop);

                    findModelChild.Index--;
                    findModelChild.MarkLabel(afterPositionIndicator);

                    findModelChild.MarkLabel(retLabel);
                }
            }
        }
    }
}
#endif