using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2Randomizer.PrefabMarkers
{
    public class VoidCradleMarker : MonoBehaviour
    {
        [SystemInitializer]
        static void Init()
        {
            AsyncOperationHandle<GameObject> voidCrablePrefabAssetRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidChest/VoidChest.prefab");
            voidCrablePrefabAssetRequest.Completed += static handle =>
            {
                GameObject voidCradlePrefab = handle.Result;
                if (voidCradlePrefab)
                {
                    voidCradlePrefab.AddComponent<VoidCradleMarker>();

#if DEBUG
                    Log.Debug($"added void cradle marker to {voidCradlePrefab}");
#endif
                }
                else
                {
                    Log.Warning("could not find void cradle prefab");
                }
            };
        }
    }
}
