using RoR2;
using RoR2Randomizer.Configuration;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RoR2Randomizer.Patches.StageEvents
{
    [PatchClass]
    static class SetStageEventOverride
    {
        static FamilyDirectorCardCategorySelection _gupFamilySelection;

        static SetStageEventOverride()
        {
            AsyncOperationHandle<FamilyDirectorCardCategorySelection> gupFamilyAssetRequest = Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGupFamily.asset");
            gupFamilyAssetRequest.Completed += static (AsyncOperationHandle<FamilyDirectorCardCategorySelection> handle) =>
            {
                _gupFamilySelection = handle.Result;

#if DEBUG
                Log.Debug($"Loaded {_gupFamilySelection}");
#endif
            };
        }

        static void Apply()
        {
            On.RoR2.ClassicStageInfo.Awake += ClassicStageInfo_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.ClassicStageInfo.Awake -= ClassicStageInfo_Awake;
        }

        static void ClassicStageInfo_Awake(On.RoR2.ClassicStageInfo.orig_Awake orig, ClassicStageInfo self)
        {
            OverrideStageEventPatch.ForcedCategorySelection = null;

            if (NetworkServer.active)
            {
                if (ConfigManager.Fun.GupModeActive)
                {
                    if (!_gupFamilySelection)
                    {
                        Log.Warning($"Gup mode is enabled, but {nameof(_gupFamilySelection)} is not loaded!");
                    }
                    else
                    {
                        OverrideStageEventPatch.ForcedCategorySelection = _gupFamilySelection;
                    }
                }
            }

            orig(self);
        }
    }
}
