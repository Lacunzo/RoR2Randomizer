#if !DISABLE_SKILL_RANDOMIZER
using EntityStates;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill.NextSkillOwnerSetters.RoR2.Skills
{
    public static class SkillDef
    {
        public static void Apply()
        {
            On.RoR2.Skills.SkillDef.InstantiateNextState += SkillDef_InstantiateNextState;
        }

        public static void Cleanup()
        {
            On.RoR2.Skills.SkillDef.InstantiateNextState -= SkillDef_InstantiateNextState;
        }

        static EntityState SkillDef_InstantiateNextState(On.RoR2.Skills.SkillDef.orig_InstantiateNextState orig, global::RoR2.Skills.SkillDef self, global::RoR2.GenericSkill skillSlot)
        {
            EntityStateOwnerTracker.SkillOwnerForNextCall = skillSlot;
            return orig(self, skillSlot);
        }
    }
}
#endif