using RoR2Randomizer.ChildTransformAdditions;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.VoidRaidCrab
{
    [PatchClass]
    public static class EscapeDeath
    {
        static void Apply()
        {
            On.EntityStates.VoidRaidCrab.EscapeDeath.OnEnter += EscapeDeath_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.VoidRaidCrab.EscapeDeath.OnEnter -= EscapeDeath_OnEnter;
        }

        static void EscapeDeath_OnEnter(On.EntityStates.VoidRaidCrab.EscapeDeath.orig_OnEnter orig, global::EntityStates.VoidRaidCrab.EscapeDeath self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, self.gauntletEntranceChildName);

            orig(self);
        }
    }
}
