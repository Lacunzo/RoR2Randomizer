using EntityStates.Toolbot;
using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer.Heretic
{
    [PatchClass]
    static class Heretic_MULT_Fix
    {
        static void Apply()
        {
            On.EntityStates.Toolbot.ToolbotStanceBase.SetPrimarySkill += ToolbotStanceBase_SetPrimarySkill;
        }

        static void Cleanup()
        {
            On.EntityStates.Toolbot.ToolbotStanceBase.SetPrimarySkill -= ToolbotStanceBase_SetPrimarySkill;
        }

        static void ToolbotStanceBase_SetPrimarySkill(On.EntityStates.Toolbot.ToolbotStanceBase.orig_SetPrimarySkill orig, ToolbotStanceBase self)
        {
            if (self != null)
            {
                EntityStateMachine outer = self.outer;
                if (outer && outer.TryGetComponent<CharacterBody>(out CharacterBody body))
                {
                    GameObject masterObject = body.masterObject;
                    if (masterObject && masterObject.TryGetComponent<ExplicitSpawnReplacementInfo>(out ExplicitSpawnReplacementInfo explicitSpawnReplacement) && explicitSpawnReplacement.OriginalMasterIndex == Caches.Masters.Heretic)
                    {
                        return;
                    }
                }
            }

            orig(self);
        }
    }
}
