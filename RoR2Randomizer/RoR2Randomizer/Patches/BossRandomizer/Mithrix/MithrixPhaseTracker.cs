﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer.Mithrix
{
    [PatchClass]
    public sealed class MithrixPhaseTracker : BossPhaseTracker<MithrixPhaseTracker>
    {
        public MithrixPhaseTracker() : base("Mithrix")
        {
        }

        static void ApplyPatches()
        {
            new MithrixPhaseTracker().applyPatches();
        }

        static void CleanupPatches()
        {
            Instance?.cleanupPatches();
        }

        protected override void applyPatches()
        {
            base.applyPatches();

            IL.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseState_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter += PreEncounter_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += EncounterFinished_OnEnter;
        }

        protected override void cleanupPatches()
        {
            base.cleanupPatches();

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
                    Phase = (uint)instance.phase;
                });
            }
        }
    }
}
