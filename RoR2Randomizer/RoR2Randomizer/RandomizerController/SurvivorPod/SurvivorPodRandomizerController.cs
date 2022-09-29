using EntityStates;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.SurvivorPod
{
    public class SurvivorPodRandomizerController : Singleton<SurvivorPodRandomizerController>
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

        static Dictionary<BodyIndex, SpawnPodPrefabData> _overrideSpawnPodPrefabs;

        protected override void Awake()
        {
            base.Awake();

            Run.onRunStartGlobal += runStarted;
            Run.onRunDestroyGlobal += runDestroyed;
        }

        void OnDestroy()
        {
            Run.onRunStartGlobal -= runStarted;
            Run.onRunDestroyGlobal -= runDestroyed;
        }

        static void runStarted(Run instance)
        {
            if (ConfigManager.Misc.SurvivorPodRandomizerEnabled && NetworkServer.active)
            {
                _overrideSpawnPodPrefabs = new Dictionary<BodyIndex, SpawnPodPrefabData>();

                foreach (CharacterBody body in _bodiesWithPods.Get)
                {
                    SpawnPodPrefabData defaultSpawnData = getDefaultSpawnData(body);
                    _overrideSpawnPodPrefabs.Add(body.bodyIndex, _distinctPodPrefabs.Get.PickWeighted(s => s == defaultSpawnData ? 0.5f : 1f));
                }
            }
        }

        static void runDestroyed(Run instance)
        {
            _overrideSpawnPodPrefabs = null;
        }

        public static void TryOverrideIntroAnimation(CharacterBody body)
        {
            if (NetworkServer.active && _overrideSpawnPodPrefabs != null && _overrideSpawnPodPrefabs.TryGetValue(body.bodyIndex, out SpawnPodPrefabData replacementPod))
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
