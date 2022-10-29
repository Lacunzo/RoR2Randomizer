using RoR2;
using RoR2.Skills;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer.Heretic
{
    [PatchClass]
    static class Heretic_ConditionalSkillOverrideFix
    {
        static void Apply()
        {
            On.RoR2.GenericSkill.SetSkillOverride += GenericSkill_SetSkillOverride;
            On.RoR2.GenericSkill.UnsetSkillOverride += GenericSkill_UnsetSkillOverride;
        }

        static void Cleanup()
        {
            On.RoR2.GenericSkill.SetSkillOverride -= GenericSkill_SetSkillOverride;
            On.RoR2.GenericSkill.UnsetSkillOverride -= GenericSkill_UnsetSkillOverride;
        }

        static bool isSkillOwnedByRandomizedHeretic(GenericSkill skill)
        {
            if (skill)
            {
                CharacterBody body = skill.characterBody;
                if (body)
                {
                    GameObject master = body.masterObject;
                    if (master && master.TryGetComponent<ExplicitSpawnReplacementInfo>(out ExplicitSpawnReplacementInfo explicitSpawnReplacement) && explicitSpawnReplacement.OriginalMasterIndex == Caches.Masters.Heretic)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static void GenericSkill_UnsetSkillOverride(On.RoR2.GenericSkill.orig_UnsetSkillOverride orig, GenericSkill self, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority)
        {
            if (priority == GenericSkill.SkillOverridePriority.Contextual && isSkillOwnedByRandomizedHeretic(self))
                return;

            orig(self, source, skillDef, priority);
        }

        static void GenericSkill_SetSkillOverride(On.RoR2.GenericSkill.orig_SetSkillOverride orig, GenericSkill self, object source, SkillDef skillDef, GenericSkill.SkillOverridePriority priority)
        {
            if (priority == GenericSkill.SkillOverridePriority.Contextual && isSkillOwnedByRandomizedHeretic(self))
                return;

            orig(self, source, skillDef, priority);
        }
    }
}
