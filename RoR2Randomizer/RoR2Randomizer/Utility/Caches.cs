using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public static class Caches
    {
        public static readonly InitializeOnAccessDictionary<string, CharacterMaster> MasterPrefabs = new InitializeOnAccessDictionary<string, CharacterMaster>(name => MasterCatalog.FindMasterPrefab(name)?.GetComponent<CharacterMaster>());

        public static readonly InitializeOnAccessDictionary<GameObject, float> CharacterBodyRadius = new InitializeOnAccessDictionary<GameObject, float>((GameObject bodyPrefab, out float radius) =>
        {
            if (bodyPrefab.TryGetComponent<SphereCollider>(out SphereCollider sphereCollider))
            {
                radius = sphereCollider.radius;
                return true;
            }
            else if (bodyPrefab.TryGetComponent<CapsuleCollider>(out CapsuleCollider capsuleCollider))
            {
                radius = capsuleCollider.radius;
                return true;
            }
            else
            {
                radius = -1f;
                return false;
            }
        });

        public static class Scene
        {
            public static SceneIndex ArtifactTrialSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex NewtShopSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex GoldShoresSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex ObliterateSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex LunarScavFightSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex CommencementSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex VoidlingFightSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex VoidLocusSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex AbandonedAqueductSceneIndex { get; private set; } = SceneIndex.Invalid;

            [SystemInitializer(typeof(SceneCatalog))]
            static void Init()
            {
                ArtifactTrialSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.ARTIFACT_TRIAL_SCENE_NAME);
#if DEBUG
                if (ArtifactTrialSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.ARTIFACT_TRIAL_SCENE_NAME}'");
#endif

                NewtShopSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.NEWT_SHOP_SCENE_NAME);
#if DEBUG
                if (NewtShopSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.NEWT_SHOP_SCENE_NAME}'");
#endif

                GoldShoresSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.GOLD_SHORES_SCENE_NAME);
#if DEBUG
                if (GoldShoresSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.GOLD_SHORES_SCENE_NAME}'");
#endif

                ObliterateSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.OBLITERATE_SCENE_NAME);
#if DEBUG
                if (ObliterateSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.OBLITERATE_SCENE_NAME}'");
#endif

                LunarScavFightSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME);
#if DEBUG
                if (LunarScavFightSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME}'");
#endif

                CommencementSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.COMMENCEMENT_SCENE_NAME);
#if DEBUG
                if (CommencementSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.COMMENCEMENT_SCENE_NAME}'");
#endif

                VoidlingFightSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME);
#if DEBUG
                if (VoidlingFightSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME}'");
#endif

                VoidLocusSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.VOID_LOCUS_SCENE_NAME);
#if DEBUG
                if (VoidLocusSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.VOID_LOCUS_SCENE_NAME}'");
#endif

                AbandonedAqueductSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.ABANDONED_AQUEDUCT_SCENE_NAME);
#if DEBUG
                if (AbandonedAqueductSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.ABANDONED_AQUEDUCT_SCENE_NAME}'");
#endif
            }
        }
    }
}
