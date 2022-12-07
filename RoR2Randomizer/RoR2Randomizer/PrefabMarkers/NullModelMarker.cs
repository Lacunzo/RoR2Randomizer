using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2Randomizer.PrefabMarkers
{
    public sealed class NullModelMarker : MonoBehaviour
    {
        [SystemInitializer]
        static void Init()
        {
            const string LOG_PREFIX = $"{nameof(NullModelMarker)}.{nameof(Init)} ";

            AsyncOperationHandle<GameObject> nullModelAssetRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Core/NullModel.prefab");
            GameObject nullModel = nullModelAssetRequest.WaitForCompletion();
            if (nullModel)
            {
                nullModel.AddComponent<NullModelMarker>();
#if DEBUG
                Log.Debug(LOG_PREFIX + $"added {nameof(NullModelMarker)} to {nullModel}");
#endif
            }
            else
            {
                Log.Warning(LOG_PREFIX + "NullModel asset is null");
            }
        }
    }
}
