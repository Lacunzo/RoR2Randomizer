#if !DISABLE_SKILL_RANDOMIZER
using EntityStates;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill.NextSkillOwnerSetters.RoR2.Skills
{
    [PatchClass]
    public static class ConditionalSkillDef
    {
        static void Apply()
        {
            On.RoR2.Skills.ConditionalSkillDef.InstantiateNextState += ConditionalSkillDef_InstantiateNextState;
        }

        static void Cleanup()
        {
            On.RoR2.Skills.ConditionalSkillDef.InstantiateNextState -= ConditionalSkillDef_InstantiateNextState;
        }

        static EntityState ConditionalSkillDef_InstantiateNextState(On.RoR2.Skills.ConditionalSkillDef.orig_InstantiateNextState orig, global::RoR2.Skills.ConditionalSkillDef self, GenericSkill skillSlot)
        {
            EntityStateOwnerTracker.SkillOwnerForNextCall = skillSlot;
            return orig(self, skillSlot);
        }
    }
}
#endif