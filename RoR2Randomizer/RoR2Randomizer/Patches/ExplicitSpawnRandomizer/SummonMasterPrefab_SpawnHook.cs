using RoR2;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class SummonMasterPrefab_SpawnHook
    {
        static void Apply()
        {
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster += SummonMasterBehavior_OpenSummonReturnMaster;
        }

        static void Cleanup()
        {
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster -= SummonMasterBehavior_OpenSummonReturnMaster;
        }

        static CharacterMaster SummonMasterBehavior_OpenSummonReturnMaster(On.RoR2.SummonMasterBehavior.orig_OpenSummonReturnMaster orig, SummonMasterBehavior self, Interactor activator)
        {
            bool hasReplaced = ExplicitSpawnRandomizerController.TryReplaceSummon(ref self.masterPrefab, out GameObject originalPrefab);

            CharacterMaster result = orig(self, activator);

            if (hasReplaced)
            {
                self.masterPrefab = originalPrefab;

                if (result && originalPrefab && originalPrefab.TryGetComponent<CharacterMaster>(out CharacterMaster originalMasterPrefab))
                {
                    ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(result.gameObject, originalMasterPrefab.masterIndex);
                }
            }

            return result;
        }
    }
}
