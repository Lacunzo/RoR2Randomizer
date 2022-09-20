using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Patches;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Stage
{
    public class StageRandomizerController : Singleton<StageRandomizerController>
    {
        public const string ARTIFACT_TRIAL_SCENE_NAME = "artifactworld";
        public const string COMMENCEMENT_SCENE_NAME = "moon2";
        public const string GOLD_SHORES_SCENE_NAME = "goldshores";
        public const string OBLITERATE_SCENE_NAME = "limbo";
        public const string LUNAR_SCAV_FIGHT_SCENE_NAME = "mysteryspace";
        public const string VOIDLING_FIGHT_SCENE_NAME = "voidraid";
        public const string VOID_FIELDS_SCENE_NAME = "arena";

        static readonly string[] _excludeScenes = new string[]
        {
            // Simulacrum maps
            "itancientloft",
            "itdampcave",
            "itfrozenwall",
            "itgolemplains",
            "itgoolake",
            "itmoon",
            "itskymeadow"
        };

        static readonly string[] _forceIncludeScenes = new string[]
        {
            ARTIFACT_TRIAL_SCENE_NAME,
            "bazaar",
            GOLD_SHORES_SCENE_NAME,
            OBLITERATE_SCENE_NAME,
            LUNAR_SCAV_FIGHT_SCENE_NAME
        };

        static readonly InitializeOnAccess<StageRandomizingInfo[]> _stages = new InitializeOnAccess<StageRandomizingInfo[]>(() =>
        {
            return SceneCatalog.allStageSceneDefs
            .Select(s => s.cachedName)
            .Except(_excludeScenes)
            .Concat(_forceIncludeScenes)
            .Select(name =>
            {
                StageFlags flags = StageFlags.None;

                switch (name)
                {
                    case COMMENCEMENT_SCENE_NAME:
                    case OBLITERATE_SCENE_NAME:
                    case LUNAR_SCAV_FIGHT_SCENE_NAME:
                    case VOIDLING_FIGHT_SCENE_NAME:
                        flags |= StageFlags.FirstStageBlacklist;
                        break;
                }

                return new StageRandomizingInfo(name, flags);
            }).ToArray();
        });

        static ReplacementDictionary<string> _stageReplacements;

        void Awake()
        {
            SceneCatalog.onMostRecentSceneDefChanged += sceneLoaded;

            StageRandomizerPatcher.Apply();
        }

        void OnDestroy()
        {
            SceneCatalog.onMostRecentSceneDefChanged -= sceneLoaded;

            StageRandomizerPatcher.Cleanup();
        }

        void sceneLoaded(SceneDef scene)
        {
            if (ConfigManager.StageRandomizer.Enabled)
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
            if (ConfigManager.StageRandomizer.Enabled)
            {
#if DEBUG
                Log.Debug($"First stage: {firstStageSceneName}");

                foreach (SceneDef nonStageScene in SceneCatalog.allSceneDefs.Where(s => Array.FindIndex(_stages.Get, st => st.SceneName == s.cachedName) == -1))
                {
                    Log.Debug($"Excluded scene: {nonStageScene.cachedName}");
                }
#endif

                _stageReplacements = ReplacementDictionary<string>.CreateFrom(_stages.Get, s => s.SceneName, (key, value) =>
                {
                    if (ConfigManager.StageRandomizer.FirstStageBlacklistEnabled && key.SceneName == firstStageSceneName && (value.Flags & StageFlags.FirstStageBlacklist) != 0)
                    {
#if DEBUG
                        Log.Debug($"Not allowing stage replacement {key.SceneName} -> {value.SceneName}: Candidate cannot be the first stage");
#endif
                        return false;
                    }

                    return true;
                });
            }
            else
            {
                _stageReplacements = null;
            }
        }

        public static bool TryGetReplacementSceneName(string original, out string replacement)
        {
            if (ConfigManager.StageRandomizer.Enabled && _stageReplacements != null)
            {
                return _stageReplacements.TryGetReplacement(original, out replacement);
            }

            replacement = default;
            return false;
        }

        public static bool TryGetReplacementSceneDef(SceneDef original, out SceneDef replacement)
        {
            if (TryGetReplacementSceneName(original.cachedName, out string replacementSceneName))
            {
                replacement = SceneCatalog.FindSceneDef(replacementSceneName);
                return (bool)replacement;
            }

            replacement = default;
            return false;
        }

        public static bool TryGetOriginalSceneDef(SceneDef replacement, out SceneDef originalScene)
        {
            if (ConfigManager.StageRandomizer.Enabled)
            {
                if (replacement && _stageReplacements != null && _stageReplacements.TryGetOriginal(replacement.cachedName, out string originalSceneName))
                {
                    originalScene = SceneCatalog.FindSceneDef(originalSceneName);
                    return (bool)originalScene;
                }
            }

            originalScene = default;
            return false;
        }
    }
}
