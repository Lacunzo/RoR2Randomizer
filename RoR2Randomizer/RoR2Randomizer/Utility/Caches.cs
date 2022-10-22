using HG;
using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

            public static SceneIndex OldCommencementSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex AITestSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static SceneIndex TestSceneSceneIndex { get; private set; } = SceneIndex.Invalid;

            public static ReadOnlyArray<SceneIndex> SimulacrumStageIndices { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsSimulacrumStage(SceneDef scene)
            {
                return IsSimulacrumStage(scene.sceneDefIndex);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsSimulacrumStage(SceneIndex index)
            {
                return ReadOnlyArray<SceneIndex>.BinarySearch(SimulacrumStageIndices, index) >= 0;
            }

            public static ReadOnlyArray<SceneIndex> PossibleStartingStagesIndices { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsPossibleStartingStage(SceneDef scene)
            {
                return IsPossibleStartingStage(scene.sceneDefIndex);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsPossibleStartingStage(SceneIndex index)
            {
                return ReadOnlyArray<SceneIndex>.BinarySearch(PossibleStartingStagesIndices, index) >= 0;
            }

            [SystemInitializer(typeof(SceneCatalog), typeof(GameModeCatalog))]
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

                OldCommencementSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.OLD_COMMENCEMENT_SCENE_NAME);
#if DEBUG
                if (OldCommencementSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.OLD_COMMENCEMENT_SCENE_NAME}'");
#endif

                AITestSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.AI_TEST_SCENE_NAME);
#if DEBUG
                if (AITestSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.AI_TEST_SCENE_NAME}'");
#endif

                TestSceneSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.TESTSCENE_SCENE_NAME);
#if DEBUG
                if (TestSceneSceneIndex == SceneIndex.Invalid) Log.Warning($"Unable to find scene index '{Constants.SceneNames.TESTSCENE_SCENE_NAME}'");
#endif

                HashSet<SceneIndex> simulacrumStageIndices = new HashSet<SceneIndex>();

                const string IT_RUN_NAME = "InfiniteTowerRun";
                Run itRunPrefab = GameModeCatalog.FindGameModePrefabComponent(IT_RUN_NAME);
                if (itRunPrefab)
                {
                    SceneCollection startingSceneGroup = itRunPrefab.startingSceneGroup;
                    if (startingSceneGroup)
                    {
                        static void handleCollection(SceneCollection collection, in HashSet<SceneCollection> alreadyHandledCollections, in HashSet<SceneIndex> stageIndices)
                        {
                            if (alreadyHandledCollections.Add(collection))
                            {
                                foreach (SceneCollection.SceneEntry entry in collection.sceneEntries)
                                {
                                    SceneDef scene = entry.sceneDef;
                                    if (scene)
                                    {
                                        stageIndices.Add(scene.sceneDefIndex);

                                        SceneCollection destinationsGroup = scene.destinationsGroup;
                                        if (destinationsGroup)
                                        {
                                            handleCollection(destinationsGroup, alreadyHandledCollections, stageIndices);
                                        }
                                    }
                                }
                            }
                        }

                        handleCollection(startingSceneGroup, new HashSet<SceneCollection>(), simulacrumStageIndices);
                    }
                }
                else
                {
                    Log.Warning($"Unable to find run prefab '{IT_RUN_NAME}'");
                }

                SimulacrumStageIndices = new ReadOnlyArray<SceneIndex>(simulacrumStageIndices.Where(i => i != SceneIndex.Invalid).OrderBy(i => i).ToArray());
                
                SceneIndex[] possibleStartingStages = Array.Empty<SceneIndex>();

                const string CLASSIC_RUN_NAME = "ClassicRun";
                Run runPrefab = GameModeCatalog.FindGameModePrefabComponent(CLASSIC_RUN_NAME);
                if (runPrefab)
                {
                    SceneCollection startingSceneGroup = runPrefab.startingSceneGroup;
                    if (startingSceneGroup)
                    {
                        ReadOnlyArray<SceneCollection.SceneEntry> sceneEntries = startingSceneGroup.sceneEntries;
                        possibleStartingStages = new SceneIndex[sceneEntries.Length];
                        for (int i = 0; i < sceneEntries.Length; i++)
                        {
                            possibleStartingStages[i] = sceneEntries[i].sceneDef.sceneDefIndex;
                        }
                    }
                }
                else
                {
                    Log.Warning($"Unable to find run prefab '{CLASSIC_RUN_NAME}'");
                }

                PossibleStartingStagesIndices = new ReadOnlyArray<SceneIndex>(possibleStartingStages.Where(i => i != SceneIndex.Invalid).Distinct().OrderBy(i => i).ToArray());
            }
        }
    }
}
