using EntityStates;
using HarmonyLib;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.BrotherMonster
{
    public static class SpellBaseState
    {
        public static void Apply()
        {
            IL.EntityStates.BrotherMonster.SpellBaseState.OnEnter += SpellBaseState_OnEnter;
        }

        public static void Cleanup()
        {
            IL.EntityStates.BrotherMonster.SpellBaseState.OnEnter -= SpellBaseState_OnEnter;
        }
        
        // Fix nullref if HammerRenderer doesn't exist
        static void SpellBaseState_OnEnter(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchLdstr("HammerRenderer"),
                              x => x.MatchCall<BaseState>(nameof(BaseState.FindModelChild)),
                              x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject)))))
            {
                c.Index += 2; // Move to before get_gameObject call

                c.Remove(); // Remove get_gameObject call
                Shared.Try_get_gameObject(c);
            }
        }
    }
}
