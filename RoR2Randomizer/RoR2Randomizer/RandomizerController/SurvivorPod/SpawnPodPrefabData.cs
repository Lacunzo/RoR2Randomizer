using EntityStates;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RoR2Randomizer.RandomizerController.SurvivorPod
{
    public readonly struct SpawnPodPrefabData
    {
        public readonly bool IsSpawnState;

        public readonly GameObject PodPrefab;

        public readonly SerializableEntityStateType SpawnState;

        public SpawnPodPrefabData(GameObject obj) : this(false, obj, default)
        {
        }

        public SpawnPodPrefabData(SerializableEntityStateType spawnState) : this(true, null, spawnState)
        {
        }

        SpawnPodPrefabData(bool isSpawnState, GameObject podPrefab, SerializableEntityStateType spawnState)
        {
            IsSpawnState = isSpawnState;
            PodPrefab = podPrefab;
            SpawnState = spawnState;
        }

        public static bool operator ==(SpawnPodPrefabData a, SpawnPodPrefabData b)
        {
            return EqualityComparer.Equals(a, b);
        }

        public static bool operator !=(SpawnPodPrefabData a, SpawnPodPrefabData b)
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
