using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.BossRandomizer.AlloyWorshipUnit
{
    [PatchClass]
    public sealed class AlloyWorshipUnitFightTracker : BossTracker<AlloyWorshipUnitFightTracker>
    {
        static void ApplyPatches()
        {
            new AlloyWorshipUnitFightTracker().applyPatches();
        }

        static void CleanupPatches()
        {
            Instance?.cleanupPatches();
        }

        protected override void applyPatches()
        {
            base.applyPatches();

            IL.EntityStates.Missions.SuperRoboBallEncounter.Listening.FixedUpdate += Listening_FixedUpdate;
        }

        protected override void cleanupPatches()
        {
            base.cleanupPatches();

            IL.EntityStates.Missions.SuperRoboBallEncounter.Listening.FixedUpdate -= Listening_FixedUpdate;
        }

        void Listening_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<ScriptedCombatEncounter>(nameof(ScriptedCombatEncounter.BeginEncounter))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate((ScriptedCombatEncounter encounter) =>
                {
                    if (!SpawnCardTracker.AlloyWorshipUnitSpawnCard)
                    {
                        SpawnCardTracker.AlloyWorshipUnitSpawnCard = encounter.spawns[0].spawnCard;
                    }

                    IsInFight.Value = true;

                    CombatSquad squad = encounter.combatSquad;
                    void onDefeatedServer()
                    {
                        IsInFight.Value = false;

                        if (squad)
                        {
                            squad.onDefeatedServer -= onDefeatedServer;
                        }
                    }

                    squad.onDefeatedServer += onDefeatedServer;
                });
            }
        }
    }
}
