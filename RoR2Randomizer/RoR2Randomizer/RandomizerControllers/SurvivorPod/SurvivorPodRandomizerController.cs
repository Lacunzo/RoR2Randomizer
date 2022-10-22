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
    public class SurvivorPodRandomizerController : BaseRandomizerController
    {
        static CharacterBody[] _bodiesWithPods;
        static SpawnPodPrefabData[] _distinctPodPrefabs;

        [SystemInitializer(typeof(BodyCatalog))]
        static void Init()
        {
            IEnumerable<CharacterBody> bodiesWithPods = BodyCatalog.allBodyPrefabBodyBodyComponents.Where(b => b && (b.preferredPodPrefab || !b.preferredInitialStateType.IsNothing()));
            _bodiesWithPods = bodiesWithPods.ToArray();

            _distinctPodPrefabs = bodiesWithPods.Select(getDefaultSpawnData).Distinct(SpawnPodPrefabData.EqualityComparer).ToArray();
        }

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
            if (shouldBeActive)
            {
                result = new Dictionary<BodyIndex, SpawnPodPrefabData>();

                foreach (CharacterBody body in _bodiesWithPods)
                {
                    SpawnPodPrefabData defaultSpawnData = getDefaultSpawnData(body);
                    result.Add(body.bodyIndex, _distinctPodPrefabs.PickWeighted(s => s == defaultSpawnData ? 0.5f : 1f));
                }

                return true;
            }

            result = default;
            return false;
        });

        static bool shouldBeActive => NetworkServer.active && ConfigManager.Misc.SurvivorPodRandomizerEnabled;
        public override bool IsRandomizerEnabled => shouldBeActive;

        protected override bool isNetworked => false;

        void OnDestroy()
        {
            _overrideSpawnPodPrefabs.Dispose();
        }

        public static void TryOverrideIntroAnimation(CharacterBody body)
        {
            if (shouldBeActive && _overrideSpawnPodPrefabs.HasValue && _overrideSpawnPodPrefabs.Value.TryGetValue(body.bodyIndex, out SpawnPodPrefabData replacementPod))
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
