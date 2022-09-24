#if DEBUG
using RoR2;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.Debug
{
    public class SpawnDisabler : Singleton<SpawnDisabler>
    {
        static bool _spawnsDisabled;

        protected override void Awake()
        {
            base.Awake();

            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
        }

        void OnDestroy()
        {
            On.RoR2.CombatDirector.Spawn -= CombatDirector_Spawn;
        }

        public void ToggleSpawnsDisabled()
        {
            _spawnsDisabled = !_spawnsDisabled;
            Chat.AddMessage($"Spawns {(_spawnsDisabled ? "Disabled" : "Enabled")}");
        }

        static bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, CombatDirector self, SpawnCard spawnCard, EliteDef eliteDef, Transform spawnTarget, DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, DirectorPlacementRule.PlacementMode placementMode)
        {
            if (_spawnsDisabled)
                return false;

            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }
    }
}
#endif
