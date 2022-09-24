using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    public sealed class MithrixPhaseTracker : BossPhaseTracker<MithrixPhaseTracker>
    {
        public MithrixPhaseTracker() : base("Mithrix")
        {
        }

        public override void ApplyPatches()
        {
            base.ApplyPatches();

            IL.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseState_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter += PreEncounter_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += EncounterFinished_OnEnter;
        }

        public override void CleanupPatches()
        {
            base.CleanupPatches();

            IL.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter -= BrotherEncounterPhaseBaseState_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter -= PreEncounter_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter -= EncounterFinished_OnEnter;
        }

        void EncounterFinished_OnEnter(On.EntityStates.Missions.BrotherEncounter.EncounterFinished.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.EncounterFinished self)
        {
            orig(self);

            IsInFight = false;
        }

        void PreEncounter_OnEnter(On.EntityStates.Missions.BrotherEncounter.PreEncounter.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.PreEncounter self)
        {
            orig(self);

            IsInFight = true;
            Phase = 0;
        }

        void BrotherEncounterPhaseBaseState_OnEnter(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCallvirt<PhaseCounter>(nameof(PhaseCounter.GoToNextPhase))))
            {
                c.Emit(OpCodes.Dup); // Dup PhaseCounter instance

                c.Index++; // Move after GoToNextPhase call

                c.EmitDelegate((PhaseCounter instance) =>
                {
                    Phase = instance.phase;
                });
            }
        }
    }
}
