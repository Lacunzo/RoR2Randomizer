using EntityStates.Missions.BrotherEncounter;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.Utility;
using System.Linq;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    [PatchClass]
    public static class MithrixSpawnCardTracker
    {
        static void Apply()
        {
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += Phase2_OnEnter;
            IL.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter += Phase4_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter -= Phase1_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter -= Phase2_OnEnter;
            IL.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter -= Phase4_OnEnter;
        }

        static void Phase1_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, Phase1 self)
        {
            orig(self);

            if (!SpawnCardTracker.MithrixNormalSpawnCard)
            {
                SpawnCardTracker.MithrixNormalSpawnCard = self.phaseScriptedCombatEncounter.spawns[0].spawnCard;
            }
        }

        static void Phase2_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_OnEnter orig, Phase2 self)
        {
            orig(self);

            SpawnCardTracker.MithrixPhase2SpawnCards ??= self.phaseScriptedCombatEncounter.spawns.Select(s => s.spawnCard).Distinct().ToArray();
        }

        static void Phase4_OnEnter(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCall<BrotherEncounterPhaseBaseState>(nameof(BrotherEncounterPhaseBaseState.BeginEncounter))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Phase4 __instance) =>
                {
                    if (!SpawnCardTracker.MithrixHurtSpawnCard)
                    {
                        SpawnCardTracker.MithrixHurtSpawnCard = __instance.phaseScriptedCombatEncounter.spawns[0].spawnCard;
                    }
                });
            }
        }
    }
}
