using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2Randomizer.PrefabMarkers
{
    public class VoidSeedMarker : MonoBehaviour
    {
        public enum MarkerType : byte
        {
            Monsters_Interactibles,
            Props_Elites
        }

        [SerializeField]
        MarkerType _type;

        public MarkerType Type => _type;

        [SystemInitializer]
        static void Init()
        {
            AsyncOperationHandle<GameObject> voidSeedPrefabAssetRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidCamp/VoidCamp.prefab");
            voidSeedPrefabAssetRequest.Completed += static operationHandle =>
            {
                if (!operationHandle.IsValid() || !operationHandle.IsDone)
                    return;

                Transform voidSeedTransform = operationHandle.Result.transform;

                void markCampChild(string childName, MarkerType markerType)
                {
                    Transform campChild = voidSeedTransform.Find(childName);
                    if (campChild)
                    {
                        VoidSeedMarker voidSeedMarker = campChild.gameObject.AddComponent<VoidSeedMarker>();
                        voidSeedMarker._type = markerType;

#if DEBUG
                        Log.Debug($"added marker {markerType} to {operationHandle.Result.name}/{childName}");
#endif
                    }
                    else
                    {
                        Log.Warning($"unable to find child {childName}");
                    }
                }

                markCampChild("Camp 1 - Void Monsters & Interactables", MarkerType.Monsters_Interactibles);
                markCampChild("Camp 2 - Flavor Props & Void Elites", MarkerType.Props_Elites);
            };
        }
    }
}
