using RoR2Randomizer.ChildTransformAdditions;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.VoidRaidCrab
{
    [PatchClass]
    public static class DeathState
    {
        static void Apply()
        {
            On.EntityStates.VoidRaidCrab.DeathState.OnEnter += DeathState_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.VoidRaidCrab.DeathState.OnEnter -= DeathState_OnEnter;
        }

        static void DeathState_OnEnter(On.EntityStates.VoidRaidCrab.DeathState.orig_OnEnter orig, global::EntityStates.VoidRaidCrab.DeathState self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, self.initialEffectMuzzle);
            orig(self);
        }
    }
}
