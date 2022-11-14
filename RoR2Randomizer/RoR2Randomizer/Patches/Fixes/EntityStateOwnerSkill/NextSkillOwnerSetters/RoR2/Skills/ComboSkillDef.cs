#if !DISABLE_SKILL_RANDOMIZER
using EntityStates;
using RoR2;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill.NextSkillOwnerSetters.RoR2.Skills
{
    [PatchClass]
    public static class ComboSkillDef
    {
        static void Apply()
        {
            On.RoR2.Skills.ComboSkillDef.InstantiateNextState += ComboSkillDef_InstantiateNextState;
        }

        static void Cleanup()
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
#endif