using RoR2;
using RoR2Randomizer.CharacterLimiter;
using RoR2Randomizer.Configuration;
using UnityEngine;

namespace RoR2Randomizer.Patches.CharacterLimiting
{
    [PatchClass]
    static class GetDeployableSameSlotLimitPatch
    {
        static void Apply()
        {
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit += CharacterMaster_GetDeployableSameSlotLimit;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterMaster.GetDeployableSameSlotLimit -= CharacterMaster_GetDeployableSameSlotLimit;
        }

        static int CharacterMaster_GetDeployableSameSlotLimit(On.RoR2.CharacterMaster.orig_GetDeployableSameSlotLimit orig, CharacterMaster self, DeployableSlot slot)
        {
            int result = orig(self, slot);

            CharacterLimitMode spawnLimitMode = ConfigManager.Performance.SpawnLimitMode;
            if (spawnLimitMode > CharacterLimitMode.Off)
            {
                if (slot != DeployableSlot.None && self.TryGetComponent<LimitedCharacterData>(out LimitedCharacterData limitData) && limitData.DeployableType == slot)
                {
                    int generation = limitData.Generation;
                    if (generation > 0)
                    {
                        switch (spawnLimitMode)
                        {
                            case CharacterLimitMode.DecreaseByOneForEveryGeneration:
                                result -= generation;
                                break;
                            case CharacterLimitMode.HalveForEveryGeneration:
                                result = Mathf.FloorToInt(result * Mathf.Pow(0.5f, generation));
                                break;
                            case CharacterLimitMode.DisableMinionSummon:
                                result = 0;
                                break;
                        }

#if DEBUG
                        Log.Debug($"Deployable slot limit for {self.name} (gen {generation}): {result}");
#endif
                    }
                }
            }

            return Mathf.Max(result, 0);
        }
    }
}
