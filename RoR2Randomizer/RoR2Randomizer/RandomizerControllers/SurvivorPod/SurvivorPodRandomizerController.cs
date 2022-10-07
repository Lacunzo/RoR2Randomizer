using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.SurvivorPod
{
    [RandomizerController]
    public class SurvivorPodRandomizerController : MonoBehaviour
    {
        static readonly InitializeOnAccess<CharacterBody[]> _bodiesWithPods = new InitializeOnAccess<CharacterBody[]>(() =>
        {
            return BodyCatalog.allBodyPrefabBodyBodyComponents.Where(b => b && (b.preferredPodPrefab || !b.preferredInitialStateType.IsNothing())).ToArray();
        });

        static readonly InitializeOnAccess<SpawnPodPrefabData[]> _distinctPodPrefabs = new InitializeOnAccess<SpawnPodPrefabData[]>(() =>
        {
            return _bodiesWithPods.Get.Select(getDefaultSpawnData).Distinct(SpawnPodPrefabData.EqualityComparer).ToArray();
        });

        static SpawnPodPrefabData getDefaultSpawnData(CharacterBody body)
        {
            GameObject prefab = BodyCatalog.GetBodyPrefab(body.bodyIndex);
            if (prefab && prefab.TryGetComponent<CharacterBody>(out CharacterBody bodyPrefab))
            {
                if (bodyPrefab.preferredPodPrefab)
                {
                    return new SpawnPodPrefabData(bodyPrefab.preferredPodPrefab);
                }
                else
                {
                    return new SpawnPodPrefabData(bodyPrefab.preferredInitialStateType);
                }
            }

            return default;
        }

        static readonly RunSpecific<Dictionary<BodyIndex, SpawnPodPrefabData>> _overrideSpawnPodPrefabs = new RunSpecific<Dictionary<BodyIndex, SpawnPodPrefabData>>((out Dictionary<BodyIndex, SpawnPodPrefabData> result) =>
        {
            if (NetworkServer.active && ConfigManager.Misc.SurvivorPodRandomizerEnabled)
            {
                result = new Dictionary<BodyIndex, SpawnPodPrefabData>();

                foreach (CharacterBody body in _bodiesWithPods.Get)
                {
                    SpawnPodPrefabData defaultSpawnData = getDefaultSpawnData(body);
                    result.Add(body.bodyIndex, _distinctPodPrefabs.Get.PickWeighted(s => s == defaultSpawnData ? 0.5f : 1f));
                }

                return true;
            }

            result = default;
            return false;
        });

        void OnDestroy()
        {
            _overrideSpawnPodPrefabs.Dispose();
        }

        public static void TryOverrideIntroAnimation(CharacterBody body)
        {
            if (NetworkServer.active && _overrideSpawnPodPrefabs.HasValue && _overrideSpawnPodPrefabs.Value.TryGetValue(body.bodyIndex, out SpawnPodPrefabData replacementPod))
            {
                if (replacementPod.IsSpawnState)
                {
                    body.preferredInitialStateType = replacementPod.SpawnState;
                    body.preferredPodPrefab = null;
                }
                else
                {
                    body.preferredPodPrefab = replacementPod.PodPrefab;
                    body.preferredInitialStateType = default;
                }
            }
        }
    }
}
