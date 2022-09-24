using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.Assassin2
{
    // NullRef when used by non-ai character: Fixed
    // Animator states
    // HitBoxGroups
    public static class DashStrike
    {
        static readonly BullseyeSearch _search = new BullseyeSearch()
        {
            sortMode = BullseyeSearch.SortMode.Distance,
            maxDistanceFilter = 40f,
            maxAngleFilter = 20f
        };

        public static void Apply()
        {
            On.EntityStates.Assassin2.DashStrike.OnEnter += On_DashStrike_OnEnter;
            On.EntityStates.Assassin2.DashStrike.OnExit += On_DashStrike_OnExit;

            IL.EntityStates.Assassin2.DashStrike.OnEnter += IL_DashStrike_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.Assassin2.DashStrike.OnEnter -= On_DashStrike_OnEnter;
            On.EntityStates.Assassin2.DashStrike.OnExit -= On_DashStrike_OnExit;

            IL.EntityStates.Assassin2.DashStrike.OnEnter -= IL_DashStrike_OnEnter;
        }

        static void On_DashStrike_OnEnter(On.EntityStates.Assassin2.DashStrike.orig_OnEnter orig, global::EntityStates.Assassin2.DashStrike self)
        {
            Shared.TryAddTemporaryComponentIfMissing(self.characterBody, ref self.outer.commonComponents.characterMotor);

            orig(self);
        }

        static void On_DashStrike_OnExit(On.EntityStates.Assassin2.DashStrike.orig_OnExit orig, global::EntityStates.Assassin2.DashStrike self)
        {
            orig(self);

            TempComponentsTracker.TryRemoveTempComponent<CharacterMotor>(self.characterBody);
        }

        static void IL_DashStrike_OnEnter(ILContext il)
        {
            MethodInfo objectExists = ReflectionUtils.FindImplicitConversion(typeof(UnityEngine.Object), typeof(bool));

            ILCursor c = new ILCursor(il);
            
            if (c.TryGotoNext(x => x.MatchLdarg(0),
                              x => x.MatchCall(AccessTools.PropertyGetter(typeof(EntityState), nameof(EntityState.characterBody))),
                              x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(CharacterBody), nameof(CharacterBody.master))),
                              x => x.MatchCallvirt(SymbolExtensions.GetMethodInfo<Component>(_ => _.GetComponent<BaseAI>())),
                              x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(BaseAI), nameof(BaseAI.currentEnemy))),
                              x => x.MatchCallvirt(AccessTools.PropertyGetter(typeof(BaseAI.Target), nameof(BaseAI.Target.characterBody)))))
            {
                c.Index += 2; // Move to after get_characterBody

                c.RemoveRange(4); // Remove master.GetComponent<BaseAI>().currentEnemy.characterBody
                
                c.EmitDelegate((CharacterBody body) =>
                {
                    if (body.master && body.master.TryGetComponent<BaseAI>(out BaseAI ai) && ai.currentEnemy != null && ai.currentEnemy.characterBody)
                    {
                        return ai.currentEnemy.characterBody;
                    }

                    _search.teamMaskFilter = TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex);
                    _search.viewer = body;
                    _search.searchOrigin = body.inputBank.aimOrigin;
                    _search.searchDirection = body.inputBank.aimDirection;
                    _search.RefreshCandidates();
                    _search.FilterOutGameObject(body.gameObject);

                    HurtBox target = _search.GetResults().FirstOrDefault(h => h.healthComponent && h.healthComponent.body);
                    return target ? target.healthComponent.body : body;
                });
            }
        }
    }
}
