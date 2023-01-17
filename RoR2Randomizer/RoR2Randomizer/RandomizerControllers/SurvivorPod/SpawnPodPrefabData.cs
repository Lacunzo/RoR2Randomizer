using EntityStates;
using RoR2;
using RoR2Randomizer.BodyAnimationMirroring;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.SurvivorPod
{
    public readonly struct SpawnPodPrefabData
    {
        public readonly bool IsSpawnState;
        public readonly BodyIndex TargetBodyIndex;

        public readonly GameObject PodPrefab;
        public readonly SerializableEntityStateType SpawnState;

        public SpawnPodPrefabData(BodyIndex bodyIndex)
        {
            CharacterBody bodyPrefab = BodyCatalog.GetBodyPrefabBodyComponent(bodyIndex);
            if (!bodyPrefab)
            {
                Log.Error($"null body prefab at index {bodyIndex}!");
                TargetBodyIndex = BodyIndex.None;
                return;
            }

            TargetBodyIndex = bodyIndex;
            if (bodyPrefab.preferredPodPrefab)
            {
                IsSpawnState = false;
                PodPrefab = bodyPrefab.preferredPodPrefab;
            }
            else
            {
                IsSpawnState = true;
                SpawnState = bodyPrefab.preferredInitialStateType;
            }
        }

        public SpawnPodPrefabData(NetworkReader reader)
        {
            TargetBodyIndex = reader.ReadBodyIndex();

            CharacterBody bodyPrefab = BodyCatalog.GetBodyPrefabBodyComponent(TargetBodyIndex);
            if (bodyPrefab)
            {
                if (IsSpawnState = reader.ReadBoolean())
                {
                    SpawnState = bodyPrefab.preferredInitialStateType;
                }
                else
                {
                    PodPrefab = bodyPrefab.preferredPodPrefab;
                }
            }
            else
            {
                Log.Error($"null body prefab at index {TargetBodyIndex}!");
            }
        }

        public readonly void Serialize(NetworkWriter writer)
        {
            writer.WriteBodyIndex(TargetBodyIndex);
            writer.Write(IsSpawnState);
        }

        public static bool operator ==(in SpawnPodPrefabData a, in SpawnPodPrefabData b)
        {
            return EqualityComparer.Equals(a, b);
        }

        public static bool operator !=(in SpawnPodPrefabData a, in SpawnPodPrefabData b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is SpawnPodPrefabData data && EqualityComparer.Equals(this, data);
        }

        public override int GetHashCode()
        {
            return EqualityComparer.GetHashCode(this);
        }

        public readonly void OverrideIntroAnimationOnBody(CharacterBody body)
        {
            if (IsSpawnState)
            {
                SerializableEntityStateType spawnState = SpawnState;

                body.preferredInitialStateType = spawnState;
                body.preferredPodPrefab = null;

                if (TargetBodyIndex != body.bodyIndex)
                {
                    MirroredIntroAnimationTracker introAnimationTracker = body.gameObject.AddComponent<MirroredIntroAnimationTracker>();
                    introAnimationTracker.IntroState = spawnState;
                    introAnimationTracker.Network_mirrorBodyIndex = TargetBodyIndex;
                }
            }
            else
            {
                body.preferredPodPrefab = PodPrefab;
                body.preferredInitialStateType = default;
            }
        }

        public static readonly IEqualityComparer<SpawnPodPrefabData> EqualityComparer = new PodEqualityComparer();
        class PodEqualityComparer : IEqualityComparer<SpawnPodPrefabData>
        {
            public bool Equals(SpawnPodPrefabData x, SpawnPodPrefabData y)
            {
                if (x.IsSpawnState != y.IsSpawnState)
                {
                    return false;
                }
                else if (x.IsSpawnState) // Both are the same, no need to check y.IsSpawnState
                {
                    return x.SpawnState.stateType == y.SpawnState.stateType;
                }
                else
                {
                    return x.PodPrefab == y.PodPrefab;
                }
            }

            public int GetHashCode(SpawnPodPrefabData pod)
            {
                int hashCode = 1489032728;
                hashCode = (hashCode * -1521134295) + pod.IsSpawnState.GetHashCode();

                if (pod.IsSpawnState)
                {
                    hashCode = (hashCode * -1521134295) + pod.SpawnState.GetHashCode();
                }
                else
                {
                    hashCode = (hashCode * -1521134295) + pod.PodPrefab.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}
