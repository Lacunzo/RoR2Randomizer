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
    public static class MithrixPhaseTracker
    {
        static bool _isInMithrixFight;
        public static bool IsInMithrixFight
        {
            get
            {
                return _isInMithrixFight;
            }
            private set
            {
                if (MiscUtils.TryAssign(ref _isInMithrixFight, value))
                {
                    if (value)
                    {
#if DEBUG
                        MiscUtils.SendChatMessage("Enter mithrix fight", "DEBUG");
#endif

                        OnEnterMithrixFight?.Invoke();
                    }
                    else
                    {
#if DEBUG
                        MiscUtils.SendChatMessage("Exit mithrix fight", "DEBUG");
#endif

                        OnExitMithrixFight?.Invoke();
                    }
                }
            }
        }

        public static event Action OnEnterMithrixFight;
        public static event Action OnExitMithrixFight;

        static int _phase;
        public static int Phase
        {
            get
            {
                return _phase;
            }
            private set
            {
                if (MiscUtils.TryAssign(ref _phase, value))
                {
#if DEBUG
                    MiscUtils.SendChatMessage($"Enter mithrix fight phase {value}", "DEBUG");
#endif

                    OnMithrixFightPhaseChanged?.Invoke();
                }
            }
        }

        public static event Action OnMithrixFightPhaseChanged;

        public static void Apply()
        {
            IL.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseState_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter += PreEncounter_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += EncounterFinished_OnEnter;

            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        public static void Cleanup()
        {
            IL.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter -= BrotherEncounterPhaseBaseState_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.PreEncounter.OnEnter -= PreEncounter_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter -= EncounterFinished_OnEnter;

            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        static void EncounterFinished_OnEnter(On.EntityStates.Missions.BrotherEncounter.EncounterFinished.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.EncounterFinished self)
        {
            orig(self);

            IsInMithrixFight = false;
        }

        static void onSceneLoaded(SceneDef _)
        {
            IsInMithrixFight = false;
        }

        static void PreEncounter_OnEnter(On.EntityStates.Missions.BrotherEncounter.PreEncounter.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.PreEncounter self)
        {
            orig(self);

            IsInMithrixFight = true;
            Phase = 0;
        }

        static void BrotherEncounterPhaseBaseState_OnEnter(ILContext il)
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
