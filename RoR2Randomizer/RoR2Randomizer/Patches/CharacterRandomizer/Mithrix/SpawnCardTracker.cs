using EntityStates.Missions.BrotherEncounter;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoR2Randomizer.Patches.CharacterRandomizer.Mithrix
{
    public static class SpawnCardTracker
    {
        public static SpawnCard MithrixNormalSpawnCard { get; private set; }
        public static SpawnCard MithrixHurtSpawnCard { get; private set; }
        public static SpawnCard[] Phase2SpawnCards { get; private set; }

        public static void Apply()
        {
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += Phase2_OnEnter;
            IL.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter += Phase4_OnEnter;
        }

        public static void Cleanup()
        {
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter -= Phase1_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter -= Phase2_OnEnter;
            IL.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter -= Phase4_OnEnter;
        }

        static void Phase1_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, Phase1 self)
        {
            orig(self);

            if (!MithrixNormalSpawnCard)
            {
                MithrixNormalSpawnCard = self.phaseScriptedCombatEncounter.spawns[0].spawnCard;
            }
        }

        static void Phase2_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_OnEnter orig, Phase2 self)
        {
            orig(self);

            Phase2SpawnCards ??= self.phaseScriptedCombatEncounter.spawns.Select(s => s.spawnCard).Distinct().ToArray();
        }

        static void Phase4_OnEnter(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCall<BrotherEncounterPhaseBaseState>(nameof(BrotherEncounterPhaseBaseState.BeginEncounter))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((Phase4 __instance) =>
                {
                    if (!MithrixHurtSpawnCard)
                    {
                        MithrixHurtSpawnCard = __instance.phaseScriptedCombatEncounter.spawns[0].spawnCard;
                    }
                });
            }
        }
    }
}
