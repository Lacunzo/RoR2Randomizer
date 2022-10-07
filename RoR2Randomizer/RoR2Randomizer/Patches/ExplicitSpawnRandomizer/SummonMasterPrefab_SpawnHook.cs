using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using System;
using System.Collections.Generic;
using System.Text;
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
                if (originalPrefab.GetComponent<SetDontDestroyOnLoad>())
                {
                    result.gameObject.GetOrAddComponent<SetDontDestroyOnLoad>();
                }
                else if (result.TryGetComponent<SetDontDestroyOnLoad>(out SetDontDestroyOnLoad setDontDestroyOnLoad))
                {
                    GameObject.Destroy(setDontDestroyOnLoad);
                }

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
