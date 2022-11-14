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
            const string LOG_PREFIX = $"{nameof(SetStageEventOverride)}..cctor ";

            AsyncOperationHandle<FamilyDirectorCardCategorySelection> gupFamilyAssetRequest = Addressables.LoadAssetAsync<FamilyDirectorCardCategorySelection>("RoR2/Base/Common/dccsGupFamily.asset");
            gupFamilyAssetRequest.Completed += static (AsyncOperationHandle<FamilyDirectorCardCategorySelection> handle) =>
            {
                _gupFamilySelection = handle.Result;

#if DEBUG
                Log.Debug(LOG_PREFIX + $"Loaded {_gupFamilySelection}");
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
            const string LOG_PREFIX = $"{nameof(SetStageEventOverride)}.{nameof(ClassicStageInfo_Awake)} ";

            OverrideStageEventPatch.ForcedCategorySelection = null;

            if (NetworkServer.active)
            {
                if (ConfigManager.Fun.GupModeActive)
                {
                    if (!_gupFamilySelection)
                    {
                        Log.Warning(LOG_PREFIX + $" Gup mode is enabled, but {nameof(_gupFamilySelection)} is not loaded!");
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
