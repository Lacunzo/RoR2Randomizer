#if !DISABLE_SKILL_RANDOMIZER
namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.Bandit2.Weapon
{
    [PatchClass]
    public static class Reload
    {
        static void Apply()
        {
            IL.EntityStates.Bandit2.Weapon.Reload.FixedUpdate += Shared.EntityState_SkillSlotResolver;
            IL.EntityStates.Bandit2.Weapon.Reload.GiveStock += Shared.EntityState_SkillSlotResolver;
        }

        static void Cleanup()
        {
            IL.EntityStates.Bandit2.Weapon.Reload.FixedUpdate -= Shared.EntityState_SkillSlotResolver;
            IL.EntityStates.Bandit2.Weapon.Reload.GiveStock -= Shared.EntityState_SkillSlotResolver;
        }
    }
}
#endif