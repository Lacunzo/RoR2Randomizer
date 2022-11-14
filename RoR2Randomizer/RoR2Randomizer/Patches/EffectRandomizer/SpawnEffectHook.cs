using RoR2;
using RoR2Randomizer.RandomizerControllers.Effect;

namespace RoR2Randomizer.Patches.EffectRandomizer
{
    [PatchClass]
    static class SpawnEffectHook
    {
        static void Apply()
        {
            On.RoR2.EffectManager.SpawnEffect_EffectIndex_EffectData_bool += EffectManager_SpawnEffect;
        }

        static void Cleanup()
        {
            On.RoR2.EffectManager.SpawnEffect_EffectIndex_EffectData_bool -= EffectManager_SpawnEffect;
        }

        static void EffectManager_SpawnEffect(On.RoR2.EffectManager.orig_SpawnEffect_EffectIndex_EffectData_bool orig, EffectIndex effectIndex, EffectData effectData, bool transmit)
        {
            if (!transmit)
            {
                EffectRandomizerController.TryReplaceEffectIndex(ref effectIndex);
            }

            orig(effectIndex, effectData, transmit);
        }
    }
}
