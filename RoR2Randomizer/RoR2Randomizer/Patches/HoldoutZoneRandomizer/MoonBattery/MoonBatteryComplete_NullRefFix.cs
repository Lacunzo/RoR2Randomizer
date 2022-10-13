#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using HarmonyLib;
using MonoMod.Cil;
using RoR2Randomizer.Patches.Fixes;
using UnityEngine;

namespace RoR2Randomizer.Patches.HoldoutZoneRandomizer.MoonBattery
{
    [PatchClass]
    static class MoonBatteryComplete_NullRefFix
    {
        static void Apply()
        {
            IL.EntityStates.Missions.Moon.MoonBatteryComplete.OnEnter += ChargedFX_NullRefFix;
        }

        static void Cleanup()
        {
            IL.EntityStates.Missions.Moon.MoonBatteryComplete.OnEnter -= ChargedFX_NullRefFix;
        }

        static void ChargedFX_NullRefFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            while (c.TryFindNext(out foundCursors,
                                 x => x.MatchLdstr("ChargedFX"),
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