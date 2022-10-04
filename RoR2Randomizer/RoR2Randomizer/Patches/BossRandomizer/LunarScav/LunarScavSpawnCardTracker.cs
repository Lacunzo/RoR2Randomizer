using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.BossRandomizer.LunarScav
{
    [PatchClass]
    static class LunarScavSpawnCardTracker
    {
        static void Apply()
        {
            On.RoR2.ScriptedCombatEncounter.BeginEncounter += ScriptedCombatEncounter_BeginEncounter;
        }

        static void Cleanup()
        {
            On.RoR2.ScriptedCombatEncounter.BeginEncounter -= ScriptedCombatEncounter_BeginEncounter;
        }

        static void ScriptedCombatEncounter_BeginEncounter(On.RoR2.ScriptedCombatEncounter.orig_BeginEncounter orig, ScriptedCombatEncounter self)
        {
            if (!SpawnCardTracker.LunarScavSpawnCard)
            {
                if (LunarScavFightTracker.Instance != null && LunarScavFightTracker.Instance.IsInFight && self.name == "ScavLunarEncounter")
                {
                    SpawnCardTracker.LunarScavSpawnCard = self.spawns[0].spawnCard as MultiCharacterSpawnCard;
                }
            }

            orig(self);
        }
    }
}
