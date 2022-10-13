using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Stage
{
    [RandomizerController]
    public class StageRandomizerController : MonoBehaviour
    {
        static bool _isInitialized;
        static StageRandomizingInfo[] _stages;

        public static SceneIndex ArtifactTrialSceneIndex { get; private set; }

        [SystemInitializer(typeof(SceneCatalog), typeof(GameModeCatalog))]
        static void Init()
        {
            ArtifactTrialSceneIndex = SceneCatalog.FindSceneIndex(Constants.SceneNames.ARTIFACT_TRIAL_SCENE_NAME);

            SceneIndex[] excludeScenes = new SceneIndex[]
            {
                // Simulacrum maps
                SceneCatalog.FindSceneIndex("itancientloft"),
                SceneCatalog.FindSceneIndex("itdampcave"),
                SceneCatalog.FindSceneIndex("itfrozenwall"),
                SceneCatalog.FindSceneIndex("itgolemplains"),
                SceneCatalog.FindSceneIndex("itgoolake"),
                SceneCatalog.FindSceneIndex("itmoon"),
                SceneCatalog.FindSceneIndex("itskymeadow")
            };

            SceneIndex[] possibleStartingStages = null;
            Run runPrefab = PreGameController.GameModeConVar.instance.runPrefabComponent;
            if (runPrefab)
            {
                SceneCollection startingSceneGroup = runPrefab.startingSceneGroup;
                if (startingSceneGroup)
                {
                    HG.ReadOnlyArray<SceneCollection.SceneEntry> sceneEntries = startingSceneGroup.sceneEntries;
                    possibleStartingStages = new SceneIndex[sceneEntries.Length];
                    for (int i = 0; i < sceneEntries.Length; i++)
                    {
                        possibleStartingStages[i] = sceneEntries[i].sceneDef.sceneDefIndex;
                    }
                }
            }

            _stages = SceneCatalog.allStageSceneDefs
                                  .Where(s => Array.IndexOf(excludeScenes, s.sceneDefIndex) == -1)
                                  .Concat(new string[]
                                  {
                                      Constants.SceneNames.ARTIFACT_TRIAL_SCENE_NAME,
                                      "bazaar",
                                      Constants.SceneNames.GOLD_SHORES_SCENE_NAME,
                                      Constants.SceneNames.OBLITERATE_SCENE_NAME,
                                      Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME
                                  }.Select(SceneCatalog.FindSceneDef))
                                  .Select(scene =>
                                  {
                                      StageFlags flags = StageFlags.None;
                                  
                                      switch (scene.cachedName)
                                      {
                                          case Constants.SceneNames.COMMENCEMENT_SCENE_NAME:
                                          case Constants.SceneNames.LUNAR_SCAV_FIGHT_SCENE_NAME:
                                          case Constants.SceneNames.VOIDLING_FIGHT_SCENE_NAME:
                                          case Constants.SceneNames.VOID_LOCUS_SCENE_NAME:
                                              flags |= StageFlags.FirstStageBlacklist;
                                              break;
                                      }

                                      if (possibleStartingStages != null && Array.IndexOf(possibleStartingStages, scene.sceneDefIndex) != -1)
                                      {
                                          flags |= StageFlags.PossibleStartingStage;
                                      }
                                  
                                      return new StageRandomizingInfo(scene.sceneDefIndex, flags);
                                  }).ToArray();

            _isInitialized = true;
        }

        static ReplacementDictionary<SceneIndex> _stageReplacements;

        void Awake()
        {
            SceneCatalog.onMostRecentSceneDefChanged += sceneLoaded;
        }

        void OnDestroy()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= sceneLoaded;
        }

        void sceneLoaded(SceneDef scene)
        {
            if (NetworkServer.active && ConfigManager.StageRandomizer.Enabled)
            {
                StartCoroutine(waitThenCheckForStageLooping(scene));
            }
        }

        static IEnumerator waitThenCheckForStageLooping(SceneDef scene)
        {
            // Wait for Start to be called on any initializing scripts first
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            if (Run.instance && Run.instance.nextStageScene && TryGetReplacementSceneDef(Run.instance.nextStageScene, out SceneDef nextStageSceneReplacement))
            {
                // If the replacement of the next stage is the current scene, it will result in an endless loop of the same stage
                if (nextStageSceneReplacement == scene)
                {
#if DEBUG
                    Log.Debug("Stage loop detected!");
#endif

                    if (TryGetOriginalSceneDef(scene, out SceneDef originalScene))
                    {
#if DEBUG
                        Log.Debug($"Picking new stage from {originalScene.cachedName} destinations");
#endif

                        Patches.Reverse.Run.PickNextStageSceneFromSceneDestinations(Run.instance, originalScene);
                    }
                }
            }
        }

        public static void InitializeStageReplacements(string firstStageSceneName)
        {
            if (!_isInitialized)
                return;

            if (NetworkServer.active && ConfigManager.StageRandomizer.Enabled)
            {
#if DEBUG
                Log.Debug($"First stage: {firstStageSceneName}");

                foreach (SceneDef nonStageScene in SceneCatalog.allSceneDefs.Where(s => Array.FindIndex(_stages, st => st.SceneIndex == s.sceneDefIndex) == -1))
                {
                    Log.Debug($"Excluded scene: {nonStageScene.cachedName}");
                }
#endif

                SceneIndex firstStageIndex = SceneCatalog.FindSceneIndex(firstStageSceneName);
                if (firstStageIndex == SceneIndex.Invalid)
                {
                    Log.Warning($"Could not find scene index for {firstStageSceneName}");
                }

                _stageReplacements = ReplacementDictionary<SceneIndex>.CreateFrom(_stages, s => s.SceneIndex, (key, value) =>
                {
                    if (ConfigManager.StageRandomizer.FirstStageBlacklistEnabled && firstStageIndex != SceneIndex.Invalid && key.SceneIndex == firstStageIndex && (value.Flags & StageFlags.FirstStageBlacklist) != 0)
                    {
#if DEBUG
                        Log.Debug($"Not allowing stage replacement {SceneCatalog.GetSceneDef(key.SceneIndex)?.cachedName} -> {SceneCatalog.GetSceneDef(value.SceneIndex)?.cachedName}: Candidate cannot be the first stage");
#endif
                        return false;
                    }

                    return true;
                }, (key, value) =>
                {
                    float weightMultiplier = 1f;

                    // Decrease likelyhood of an "ordinary" starting stage being picked as the first stage
                    if (firstStageIndex != SceneIndex.Invalid && key.SceneIndex == firstStageIndex && (value.Flags & StageFlags.PossibleStartingStage) != 0)
                    {
                        weightMultiplier *= ConfigManager.StageRandomizer.PossibleFirstStageWeightMult;
                    }

                    return value.BaseSelectionWeight * weightMultiplier;
                });
            }
            else
            {
                _stageReplacements = null;
            }
        }

        public static bool TryGetReplacementSceneIndex(SceneIndex original, out SceneIndex replacement)
        {
            if (NetworkServer.active && ConfigManager.StageRandomizer.Enabled && _stageReplacements != null)
            {
                return _stageReplacements.TryGetReplacement(original, out replacement);
            }

            replacement = default;
            return false;
        }

        public static bool TryGetReplacementSceneName(string original, out string replacement)
        {
            SceneIndex originalIndex = SceneCatalog.FindSceneIndex(original);
            if (originalIndex != SceneIndex.Invalid)
            {
                if (TryGetReplacementSceneIndex(originalIndex, out SceneIndex replacementIndex))
                {
                    SceneDef replacementScene = SceneCatalog.GetSceneDef(replacementIndex);
                    if (replacementScene)
                    {
                        replacement = replacementScene.cachedName;
                        return true;
                    }
                }
            }

            replacement = default;
            return false;
        }

        public static bool TryGetReplacementSceneDef(SceneDef original, out SceneDef replacement)
        {
            if (TryGetReplacementSceneIndex(original.sceneDefIndex, out SceneIndex replacementSceneIndex))
            {
                replacement = SceneCatalog.GetSceneDef(replacementSceneIndex);
                return (bool)replacement;
            }

            replacement = default;
            return false;
        }

        public static bool TryGetOriginalSceneDef(SceneDef replacement, out SceneDef originalScene)
        {
            if (ConfigManager.StageRandomizer.Enabled)
            {
                if (replacement && _stageReplacements != null && _stageReplacements.TryGetOriginal(replacement.sceneDefIndex, out SceneIndex originalSceneIndex))
                {
                    originalScene = SceneCatalog.GetSceneDef(originalSceneIndex);
                    return (bool)originalScene;
                }
            }

            originalScene = default;
            return false;
        }
    }
}
