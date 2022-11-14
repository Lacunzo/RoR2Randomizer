using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.Boss;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.BrotherMonster
{
    [PatchClass]
    public static class SpellBaseState
    {
        static void Apply()
        {
            IL.EntityStates.BrotherMonster.SpellBaseState.OnEnter += SpellBaseState_OnEnter;
            IL.EntityStates.BrotherMonster.SpellBaseState.InitItemStealer += SpellBaseState_InitItemStealer;
        }

        static void Cleanup()
        {
            IL.EntityStates.BrotherMonster.SpellBaseState.OnEnter -= SpellBaseState_OnEnter;
            IL.EntityStates.BrotherMonster.SpellBaseState.InitItemStealer -= SpellBaseState_InitItemStealer;
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
                Shared.Try_get_gameObject(c);
            }
        }

        static void SpellBaseState_InitItemStealer(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(x => x.MatchCallvirt(SymbolExtensions.GetMethodInfo<GameObject>(_ => _.GetComponent<ReturnStolenItemsOnGettingHit>()))))
            {
                c.Index++; // Move after GetComponent call

                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate((ReturnStolenItemsOnGettingHit returnItems, global::EntityStates.BrotherMonster.SpellBaseState instance) =>
                {
                    if (ConfigManager.BossRandomizer.AnyMithrixRandomizerEnabled && !returnItems && instance != null)
                    {
                        GameObject bodyObj = instance.gameObject;
                        if (bodyObj && BossRandomizerController.Mithrix.IsReplacedPartOfMithrixFight(instance.characterBody.masterObject))
                        {
                            returnItems = MainMithrixReplacement.AddReturnStolenItemsOnGettingHit(bodyObj, instance.healthComponent);
                        }
                    }

                    return returnItems;
                });
            }
        }
    }
}
