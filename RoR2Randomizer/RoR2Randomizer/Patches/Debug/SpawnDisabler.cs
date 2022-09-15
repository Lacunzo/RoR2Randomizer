#if DEBUG
using RoR2;
using UnityEngine;

namespace RoR2Randomizer.Patches.Debug
{
    public class SpawnDisabler : MonoBehaviour
    {
        static bool _spawnsDisabled;

        void Awake()
        {
            On.RoR2.CombatDirector.Spawn += CombatDirector_Spawn;
        }

        void OnDestroy()
        {
            On.RoR2.CombatDirector.Spawn -= CombatDirector_Spawn;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                _spawnsDisabled = !_spawnsDisabled;

                Chat.AddMessage($"Spawns {(_spawnsDisabled ? "Disabled" : "Enabled")}");
            }
        }

        static bool CombatDirector_Spawn(On.RoR2.CombatDirector.orig_Spawn orig, RoR2.CombatDirector self, RoR2.SpawnCard spawnCard, RoR2.EliteDef eliteDef, Transform spawnTarget, RoR2.DirectorCore.MonsterSpawnDistance spawnDistance, bool preventOverhead, float valueMultiplier, RoR2.DirectorPlacementRule.PlacementMode placementMode)
        {
            if (_spawnsDisabled)
                return false;

            return orig(self, spawnCard, eliteDef, spawnTarget, spawnDistance, preventOverhead, valueMultiplier, placementMode);
        }
    }
}
#endif
