// #define DISPLAY_OOB_TRIGGERS

#if !DEBUG
#undef DISPLAY_OOB_TRIGGERS
#endif

using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Stage
{
    [RandomizerController]
    public class StageRandomizerController : BaseRandomizerController
    {
        static StageRandomizerController _instance;
        public static StageRandomizerController Instance => _instance;

        static bool _isInitialized;
        static StageRandomizingInfo[] _stages;

        [SystemInitializer(typeof(Caches.Scene), typeof(SceneCatalog))]
        static void Init()
        {
            const string LOG_PREFIX = $"{nameof(StageRandomizerController)}.{nameof(Init)} ";

            _stages = SceneCatalog.allStageSceneDefs
                                  .Where(s => !Caches.Scene.IsSimulacrumStage(s))
                                  // These are not normal stages, but will be included anyway
                                  .Concat(new SceneIndex[]
                                  {
                                      Caches.Scene.ArtifactTrialSceneIndex,
                                      Caches.Scene.NewtShopSceneIndex,
                                      Caches.Scene.GoldShoresSceneIndex,
                                      Caches.Scene.ObliterateSceneIndex,
                                      Caches.Scene.LunarScavFightSceneIndex,
                                      Caches.Scene.OldCommencementSceneIndex,
                                      Caches.Scene.AITestSceneIndex,
                                      Caches.Scene.TestSceneSceneIndex
                                  }.Where(s => s != SceneIndex.Invalid).Select(SceneCatalog.GetSceneDef))
                                  .Select(scene =>
                                  {
                                      SceneIndex index = scene.sceneDefIndex;

                                      StageFlags flags = StageFlags.None;
                                      if ((Caches.Scene.OldCommencementSceneIndex != SceneIndex.Invalid && index == Caches.Scene.OldCommencementSceneIndex) ||
                                          (Caches.Scene.AITestSceneIndex != SceneIndex.Invalid && index == Caches.Scene.AITestSceneIndex) ||
                                          (Caches.Scene.TestSceneSceneIndex != SceneIndex.Invalid && index == Caches.Scene.TestSceneSceneIndex))
                                      {
                                          flags |= StageFlags.Inaccessible;
                                      }

                                      if ((Caches.Scene.CommencementSceneIndex != SceneIndex.Invalid && index == Caches.Scene.CommencementSceneIndex) ||
                                          (Caches.Scene.LunarScavFightSceneIndex != SceneIndex.Invalid && index == Caches.Scene.LunarScavFightSceneIndex) ||
                                          (Caches.Scene.VoidlingFightSceneIndex != SceneIndex.Invalid && index == Caches.Scene.VoidlingFightSceneIndex) ||
                                          (Caches.Scene.OldCommencementSceneIndex != SceneIndex.Invalid && index == Caches.Scene.OldCommencementSceneIndex))
                                      {
                                          flags |= StageFlags.EndsRun;
                                      }

                                      if ((flags & StageFlags.EndsRun) != 0 ||
                                          (Caches.Scene.VoidLocusSceneIndex != SceneIndex.Invalid && index == Caches.Scene.VoidLocusSceneIndex))
                                      {
                                          flags |= StageFlags.FirstStageBlacklist;
                                      }

                                      if (Caches.Scene.IsPossibleStartingStage(index))
                                      {
                                          flags |= StageFlags.PossibleStartingStage;
                                      }
                                  
                                      return new StageRandomizingInfo(index, flags);
                                  }).ToArray();

            _isInitialized = true;
        }

        static IndexReplacementsCollection? _stageIndexReplacements;

        static bool shouldBeActive => NetworkServer.active && ConfigManager.StageRandomizer.Enabled;
        public override bool IsRandomizerEnabled => shouldBeActive;

        protected override bool isNetworked => false;

        protected override void Awake()
        {
            base.Awake();

            SceneCatalog.onMostRecentSceneDefChanged += sceneLoaded;

            SingletonHelper.Assign(ref _instance, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneCatalog.onMostRecentSceneDefChanged -= sceneLoaded;

            SingletonHelper.Unassign(ref _instance, this);
        }

        void sceneLoaded(SceneDef scene)
        {
            if (!shouldBeActive)
                return;

            StartCoroutine(waitThenCheckForStageLooping(scene));

            if (Caches.Scene.AITestSceneIndex != SceneIndex.Invalid && scene.sceneDefIndex == Caches.Scene.AITestSceneIndex)
            {
                const string ZONES_HOLDER_NAME = "HOLDER: Zones";
                Transform zonesHolder = (GameObject.Find(ZONES_HOLDER_NAME) ?? new GameObject(ZONES_HOLDER_NAME)).transform;

                void addOOBTrigger(Vector3 position, Vector3 scale)
                {
                    const string NAME = "OOB_TRIGGER";

                    GameObject oobObj;
#if DISPLAY_OOB_TRIGGERS
                    oobObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    oobObj.name = NAME;
#else
                    oobObj = new GameObject(NAME);
#endif

                    oobObj.layer = LayerIndex.collideWithCharacterHullOnly.intVal;

                    oobObj.transform.SetParent(zonesHolder, false);

                    oobObj.transform.position = position;
                    oobObj.transform.localScale = scale;

                    BoxCollider boxCollider;
#if DISPLAY_OOB_TRIGGERS
                    boxCollider = oobObj.GetComponent<BoxCollider>();
#else
                    boxCollider = oobObj.AddComponent<BoxCollider>();
#endif

                    boxCollider.isTrigger = true;

                    MapZone mapZone = oobObj.AddComponent<MapZone>();
                    mapZone.triggerType = MapZone.TriggerType.TriggerEnter;
                    mapZone.zoneType = MapZone.ZoneType.OutOfBounds;
                }

                addOOBTrigger(new Vector3(0f, -300f, 0f), new Vector3(1000f, 500f, 1000f));
                addOOBTrigger(new Vector3(600f, 200f, 0f), new Vector3(1000f, 500f, 1000f));
                addOOBTrigger(new Vector3(-800f, 200f, 0f), new Vector3(1000f, 500f, 1000f));
                addOOBTrigger(new Vector3(0f, 200f, 750f), new Vector3(1000f, 500f, 1000f));
                addOOBTrigger(new Vector3(0f, 200f, -750f), new Vector3(1000f, 500f, 1000f));
                addOOBTrigger(new Vector3(0f, 500f, 0f), new Vector3(1000f, 500f, 1000f));
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

            if (shouldBeActive)
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

                // HACK: The run started callback is not invoked yet at this point, so Instance.RNG is null. Just use the run seed directly
                Xoroshiro128Plus rng = new Xoroshiro128Plus(Run.instance.seed);

                _stageIndexReplacements = IndexReplacementsCollection.Create(ReplacementDictionary<SceneIndex>.CreateFrom(_stages, rng, s => s.SceneIndex, (key, value) =>
                {
                    if (ConfigManager.StageRandomizer.FirstStageBlacklistEnabled && firstStageIndex != SceneIndex.Invalid && key.SceneIndex == firstStageIndex && (value.Flags & StageFlags.FirstStageBlacklist) != 0)
                    {
#if DEBUG
                        Log.Debug($"Not allowing stage replacement {SceneCatalog.GetSceneDef(key.SceneIndex)?.cachedName} -> {SceneCatalog.GetSceneDef(value.SceneIndex)?.cachedName}: Candidate cannot be the first stage");
#endif
                        return false;
                    }

                    if ((key.Flags & StageFlags.Inaccessible) != 0 && (value.Flags & StageFlags.EndsRun) != 0)
                    {
#if DEBUG
                        Log.Debug($"Not allowing stage replacement {SceneCatalog.GetSceneDef(key.SceneIndex)?.cachedName} -> {SceneCatalog.GetSceneDef(value.SceneIndex)?.cachedName}: {nameof(StageFlags.Inaccessible)} stages cannot become {nameof(StageFlags.EndsRun)}");
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
                }), SceneCatalog.sceneDefCount);
            }
            else
            {
                _stageIndexReplacements = null;
            }
        }

        public static bool TryGetReplacementSceneIndex(SceneIndex original, out SceneIndex replacement)
        {
            if (shouldBeActive && _stageIndexReplacements.HasValue)
            {
                return _stageIndexReplacements.Value.TryGetReplacement(original, out replacement);
            }

            replacement = SceneIndex.Invalid;
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
            if (shouldBeActive)
            {
                if (replacement && _stageIndexReplacements.HasValue && _stageIndexReplacements.Value.TryGetOriginal(replacement.sceneDefIndex, out SceneIndex originalSceneIndex))
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
