using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.NewtMonster
{
    public static class KickFromShop
    {
        public static void Apply()
        {
            IL.EntityStates.NewtMonster.KickFromShop.FixedUpdate += KickFromShop_FixedUpdate;
        }

        public static void Cleanup()
        {
            IL.EntityStates.NewtMonster.KickFromShop.FixedUpdate -= KickFromShop_FixedUpdate;
        }

        static void KickFromShop_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Fix NullRef in fixedupdate if "KickOutOfShop" does not exist in the scene
            if (c.TryGotoNext(x => x.MatchLdstr("KickOutOfShop"),
                              x => x.MatchCallvirt<Transform>(nameof(Transform.Find)),
                              x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(Component), nameof(Component.gameObject)))))
            {
                c.Index += 2; // Move to before get_gameObject call

                c.Remove(); // Remove get_gameObject call
                Shared.Try_get_gameObject(c);
            }

            // Prevent Newt from becoming immune to damage if it's replacing Mithrix
            if (c.TryGotoNext(x => x.MatchCallvirt(SymbolExtensions.GetMethodInfo<Component>(_ => _.GetComponent<HurtBoxGroup>())),
                              x => x.MatchStloc(out _),
                              x => x.MatchLdloc(out _),
                              x => x.MatchImplicitConversion(typeof(UnityEngine.Object), typeof(bool)),
                              x => x.MatchBrfalse(out _)))
            {
                c.Index += 4; // Move before BrFalse

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((global::EntityStates.NewtMonster.KickFromShop instance) =>
                {
                    CharacterBody body = instance.characterBody;
                    if (body)
                    {
                        GameObject master = body.masterObject;
                        if (master)
                        {
                            return !master.GetComponent<CharacterRandomizer.Mithrix.SpawnHook.MithrixReplacement>()
                                && !master.GetComponent<CharacterRandomizer.Mithrix.SpawnHook.MithrixPhase2EnemiesReplacement>();
                        }
                    }

                    return true;
                });

                c.Emit(OpCodes.And);
            }
        }
    }
}
