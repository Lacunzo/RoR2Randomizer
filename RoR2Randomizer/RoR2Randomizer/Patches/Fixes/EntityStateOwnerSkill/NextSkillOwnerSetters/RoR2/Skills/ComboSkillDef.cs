using EntityStates;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill.NextSkillOwnerSetters.RoR2.Skills
{
    public static class ComboSkillDef
    {
        public static void Apply()
        {
            On.RoR2.Skills.ComboSkillDef.InstantiateNextState += ComboSkillDef_InstantiateNextState;
        }

        public static void Cleanup()
        {
            On.RoR2.Skills.ComboSkillDef.InstantiateNextState -= ComboSkillDef_InstantiateNextState;
        }

        static EntityState ComboSkillDef_InstantiateNextState(On.RoR2.Skills.ComboSkillDef.orig_InstantiateNextState orig, global::RoR2.Skills.ComboSkillDef self, GenericSkill skillSlot)
        {
            EntityStateOwnerTracker.SkillOwnerForNextCall = skillSlot;
            return orig(self, skillSlot);
        }
    }
}
