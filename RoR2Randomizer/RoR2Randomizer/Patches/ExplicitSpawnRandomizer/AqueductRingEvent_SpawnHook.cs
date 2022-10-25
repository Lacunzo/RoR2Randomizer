using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    // [PatchClass]
    static class AqueductRingEvent_SpawnHook
    {
        static void Apply()
        {
            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        static void Cleanup()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        static void onSceneLoaded(SceneDef scene)
        {
            if (ExplicitSpawnRandomizerController.IsActive)
            {
                if (Caches.Scene.AbandonedAqueductSceneIndex != SceneIndex.Invalid && scene.sceneDefIndex == Caches.Scene.AbandonedAqueductSceneIndex)
                {
                    Main.Instance.StartCoroutine(waitThenReplaceRingEventEnemies());
                }
            }
        }

        static IEnumerator waitThenReplaceRingEventEnemies()
        {
            yield return new WaitForSeconds(0.25f);

            GameObject holder = GameObject.Find("HOLDER: Secret Ring Area Content");
            if (holder)
            {
                Transform eventController = holder.transform.Find("ApproxCenter/RingEventController");
                if (eventController)
                {
                    foreach (CharacterMaster master in eventController.GetComponentsInChildren<CharacterMaster>())
                    {
                        if (!master.hasBody)
                        {
                            CharacterMaster replacementMasterPrefab = ExplicitSpawnRandomizerController.GetSummonReplacement(master);
                            if (replacementMasterPrefab)
                            {
                                master.bodyPrefab = replacementMasterPrefab.bodyPrefab;
                            }
                        }
                    }
                }
            }
        }
    }
}
