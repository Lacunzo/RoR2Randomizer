using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.RandomizerControllers.ExplicitSpawn;
using RoR2Randomizer.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.ExplicitSpawnRandomizer
{
    [PatchClass]
    static class Shopkeeper_SpawnHook
    {
        static void Apply()
        {
            SceneCatalog.onMostRecentSceneDefChanged += onSceneLoaded;
        }

        static void Cleanup()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= onSceneLoaded;
        }

        static void onSceneLoaded(SceneDef scene)
        {
            if (!NetworkServer.active)
                return;

            if (Caches.Scene.NewtShopSceneIndex == SceneIndex.Invalid || scene.sceneDefIndex != Caches.Scene.NewtShopSceneIndex)
                return;

            if (!Caches.Masters.ShopkeeperNewt.isValid)
                return;

            if (!ExplicitSpawnRandomizerController.IsActive || !ConfigManager.ExplicitSpawnRandomizer.RandomizeShopkeeperNewt)
                return;

            MasterCatalog.MasterIndex replacementIndex = ExplicitSpawnRandomizerController.GetSummonReplacement(Caches.Masters.ShopkeeperNewt);
            if (!replacementIndex.isValid)
                return;

            GameObject replacementPrefabObj = MasterCatalog.GetMasterPrefab(replacementIndex);
            if (!replacementPrefabObj)
                return;

            if (!replacementPrefabObj.TryGetComponent<CharacterMaster>(out CharacterMaster replacementPrefab) || !replacementPrefab.bodyPrefab)
                return;

            const string STORE_HOLDER_OBJECT_NAME = "HOLDER: Store";
            GameObject storeHolder = GameObject.Find(STORE_HOLDER_OBJECT_NAME);
            if (!storeHolder)
            {
                Log.Warning($"{nameof(Shopkeeper_SpawnHook)}.{nameof(onSceneLoaded)} could not find {STORE_HOLDER_OBJECT_NAME}");
                return;
            }

            Transform shopkeeperMasterTransform = storeHolder.transform.Find("HOLDER: Store Platforms/ShopkeeperPosition/ShopkeeperMaster");
            if (!shopkeeperMasterTransform || !shopkeeperMasterTransform.TryGetComponent<CharacterMaster>(out CharacterMaster shopKeeperMaster))
                return;
            
            shopKeeperMaster.bodyPrefab = replacementPrefab.bodyPrefab;

            ExplicitSpawnRandomizerController.RegisterSpawnedReplacement(shopKeeperMaster.gameObject, Caches.Masters.ShopkeeperNewt);

#if DEBUG
            Log.Debug("Replaced Shopkeeper");
#endif
        }
    }
}
