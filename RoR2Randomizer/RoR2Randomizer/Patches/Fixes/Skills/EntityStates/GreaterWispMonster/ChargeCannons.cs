#if !DISABLE_SKILL_RANDOMIZER
using RoR2Randomizer.ChildTransformAdditions;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.GreaterWispMonster
{
    [PatchClass]
    public static class ChargeCannons
    {
        static void Apply()
        {
            On.EntityStates.GreaterWispMonster.ChargeCannons.OnEnter += ChargeCannons_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.GreaterWispMonster.ChargeCannons.OnEnter -= ChargeCannons_OnEnter;
        }

        static void ChargeCannons_OnEnter(On.EntityStates.GreaterWispMonster.ChargeCannons.orig_OnEnter orig, global::EntityStates.GreaterWispMonster.ChargeCannons self)
        {
            CustomChildTransformManager.AutoAddChildTransforms(self, "MuzzleLeft", "MuzzleRight");
            orig(self);
        }
    }
}
#endif