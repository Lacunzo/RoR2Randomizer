using RoR2;
using RoR2Randomizer.RandomizerControllers.SniperWeakPoint;

namespace RoR2Randomizer.Patches.SniperWeakPointRandomizer
{
    [PatchClass]
    static class CharacterModel_RandomizeHurtBoxes
    {
        static void Apply()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterModel.Awake -= CharacterModel_Awake;
        }

        static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
        {
            if (self.TryGetComponent<HurtBoxGroup>(out HurtBoxGroup hurtBoxGroup))
            {
                SniperWeakPointRandomizerController.TryRandomizeSniperTargets(hurtBoxGroup, self.body);
            }

            orig(self);
        }
    }
}
