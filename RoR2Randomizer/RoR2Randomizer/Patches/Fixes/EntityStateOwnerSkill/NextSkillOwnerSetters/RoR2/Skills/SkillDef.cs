#if !DISABLE_SKILL_RANDOMIZER
using EntityStates;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill.NextSkillOwnerSetters.RoR2.Skills
{
    [PatchClass]
    public static class SkillDef
    {
        static void Apply()
        {
            On.RoR2.Skills.SkillDef.InstantiateNextState += SkillDef_InstantiateNextState;
        }

        static void Cleanup()
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