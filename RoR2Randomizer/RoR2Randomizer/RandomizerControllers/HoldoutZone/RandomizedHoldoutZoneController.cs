#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using HarmonyLib;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public sealed class RandomizedHoldoutZoneController : MonoBehaviour
    {
        static readonly FieldInfo[] _copyFields = (from field in typeof(HoldoutZoneController).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                                   where field.FieldType.IsValueType || field.FieldType == typeof(string)
                                                   where field.GetCustomAttribute(typeof(SerializeField)) != null || (field.IsPublic && field.GetCustomAttribute(typeof(NonSerializedAttribute)) == null)
                                                   select field).ToArray();

        static readonly InitializeOnAccess<BuffDef> bdVoidFogMild = new InitializeOnAccess<BuffDef>(() =>
        {
            return BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("bdVoidFogMild"));
        });

        static FogDamageController _cachedFogController;

        bool _isInitialized;

        HoldoutZoneController _controller;
        HoldoutZoneInfo _originalZoneInfo;
        HoldoutZoneInfo _replacementZoneInfo;

        public BaseZoneBehavior NonHoldoutZone { get; private set; }

        public bool TrySetZoneRadius(float value)
        {
            if (!NonHoldoutZone)
                return false;

            if (NonHoldoutZone is BoxZone)
            {
                transform.localScale = transform.localScale.normalized * value;
                return true;
            }
            else if (NonHoldoutZone is SphereZone sphere)
            {
                sphere.radius = value;
                return true;
            }
            else if (NonHoldoutZone is VerticalTubeZone tube)
            {
                tube.radius = value;
                return true;
            }
            else
            {
                Log.Warning($"{nameof(RandomizedHoldoutZoneController)}: Zone type {NonHoldoutZone.GetType().FullName} is not implemented");
                return false;
            }
        }

        HoldoutZoneStateType _state;
        public HoldoutZoneStateType State
        {
            get
            {
                return _state;
            }
            set
            {
                if (MiscUtils.TryAssign(ref _state, value))
                {
                    if (_cachedFogController && NonHoldoutZone)
                    {
                        switch (value)
                        {
                            case HoldoutZoneStateType.Invalid:
                            case HoldoutZoneStateType.Idle:
                            case HoldoutZoneStateType.IdleToCharging:
                            case HoldoutZoneStateType.Charged:
                            case HoldoutZoneStateType.Finished:
                                _cachedFogController.gameObject.SetActive(false);
                                NonHoldoutZone.enabled = false;
                                break;
                            case HoldoutZoneStateType.Charging:
                                _cachedFogController.gameObject.SetActive(true);
                                NonHoldoutZone.enabled = true;
                                break;
                        }
                    }
                }
            }
        }

        void Awake()
        {
            _controller = GetComponent<HoldoutZoneController>();
        }

        public void Initialize(HoldoutZoneInfo originalZoneInfo, HoldoutZoneInfo replacementZoneInfo)
        {
            if (_isInitialized)
                return;

            _originalZoneInfo = originalZoneInfo;
            _replacementZoneInfo = replacementZoneInfo;

            foreach (FieldInfo field in _copyFields)
            {
#if DEBUG
                Log.Debug($"{nameof(RandomizedHoldoutZoneController)}.{nameof(Initialize)}: Assigning field {field.Name} ({field.GetValue(_controller)} -> {field.GetValue(replacementZoneInfo.ControllerPrefab)})");
#endif

                field.SetValue(_controller, field.GetValue(replacementZoneInfo.ControllerPrefab));
            }

            EntityStateMachine mainStateMachine = _controller.GetComponent<EntityStateMachine>();

            AdditionalHoldoutZoneEntityStateMachine additionalStateMachine = _controller.gameObject.AddComponent<AdditionalHoldoutZoneEntityStateMachine>();
            additionalStateMachine.customName = Main.PluginGUID + "-AdditionalZoneMain";

            additionalStateMachine.initialStateType = replacementZoneInfo.StateCollection.Idle;

            additionalStateMachine.Controller = this;

            additionalStateMachine.OriginalZoneInfo = originalZoneInfo;
            additionalStateMachine.ReplacementZoneInfo = replacementZoneInfo;
            additionalStateMachine.Initialize(mainStateMachine);

            ref Renderer radiusIndicator = ref _controller.radiusIndicator;
            
            BaseZoneBehavior instanceZone = getNonHoldoutZoneBehavior(_controller);
            BaseZoneBehavior prefabZone = getNonHoldoutZoneBehavior(replacementZoneInfo.ControllerPrefab);

            if (prefabZone)
            {
                // Big TODO: The damage controller may not have been created by the stage at this point (since this is called directly from HoldoutZoneController.Awake), will probably need to delay the fog damage logic until a littlee bit later
                FogDamageController fogDamageController = getFogDamageController(replacementZoneInfo.ZoneType, false);

                bool createNewZoneComponent = !instanceZone;

                Type prefabZoneType = prefabZone.GetType();
                if (instanceZone && instanceZone.GetType() != prefabZoneType)
                {
                    createNewZoneComponent |= true;

                    if (fogDamageController)
                    {
                        fogDamageController.RemoveSafeZone(instanceZone);

                        Destroy(instanceZone);
                        instanceZone = null;
                    }
                }

                if (createNewZoneComponent)
                {
                    BaseZoneBehavior newInstanceZone = (BaseZoneBehavior)_controller.gameObject.AddComponent(prefabZoneType);
                    if (fogDamageController)
                    {
                        fogDamageController.AddSafeZone(newInstanceZone);
                    }
                    else
                    {
                        DelayedZoneReplacement.ApplyDelayedZoneReplacement(instanceZone, newInstanceZone, replacementZoneInfo.ZoneType);
                    }

                    instanceZone = newInstanceZone;
                }

                // Radius is not set properly here (ignores radius items), it will be properly set from HoldoutZoneController.FixedUpdate, but in case the controller isn't enabled, ser the radius now
                if (prefabZone is BoxZone prefabBoxZone)
                {
                    BoxZone boxZone = instanceZone as BoxZone;

                    boxZone.isInverted = false;
                }
                else if (prefabZone is SphereZone prefabSphereZone)
                {
                    SphereZone sphereZone = instanceZone as SphereZone;

                    sphereZone.radius = _controller.baseRadius;
                    sphereZone.indicatorSmoothTime = _controller.radiusSmoothTime;
                    sphereZone.isInverted = prefabSphereZone.isInverted;

                    sphereZone.rangeIndicator = Instantiate(prefabSphereZone.rangeIndicator, _controller.transform);
                }
                else if (prefabZone is VerticalTubeZone prefabTubeZone)
                {
                    VerticalTubeZone tubeZone = instanceZone as VerticalTubeZone;

                    tubeZone.radius = _controller.baseRadius;
                    tubeZone.indicatorSmoothTime = _controller.radiusSmoothTime;

                    tubeZone.rangeIndicator = Instantiate(prefabTubeZone.rangeIndicator, _controller.transform);
                }

                instanceZone.enabled = false;

                NonHoldoutZone = instanceZone;
                TrySetZoneRadius(0f);

                if (radiusIndicator)
                {
                    Destroy(radiusIndicator.gameObject);
                    radiusIndicator = null;
                }
            }
            else if (radiusIndicator)
            {
                Transform radiusIndicatorTransform = radiusIndicator.transform;

                Transform parent = radiusIndicatorTransform.parent;
                Vector3 localPosition = radiusIndicatorTransform.localPosition;
                Quaternion localRotation = radiusIndicatorTransform.localRotation;

                Destroy(radiusIndicator.gameObject);

                if (replacementZoneInfo.ControllerPrefab.radiusIndicator)
                {
                    radiusIndicator = Instantiate(replacementZoneInfo.ControllerPrefab.radiusIndicator, parent);
                    radiusIndicatorTransform = radiusIndicator.transform;
                    radiusIndicatorTransform.localPosition = localPosition;
                    radiusIndicatorTransform.localRotation = localRotation;
                }
                else
                {
                    radiusIndicator = null;
                }
            }

            if (replacementZoneInfo.ControllerPrefab.TryGetComponent<TeamFilter>(out TeamFilter replacementPrefabTeamFilter))
            {
                TeamFilter teamFilter = _controller.gameObject.GetOrAddComponent<TeamFilter>();
                teamFilter.teamIndex = replacementPrefabTeamFilter.defaultTeam; // teamIndex is initialized in Awake, which is not run for the prefab
            }

            _isInitialized = true;
        }

        readonly struct DelayedZoneReplacement
        {
            public readonly HoldoutZoneType ReplacedZoneType;

            public readonly BaseZoneBehavior Original;
            public readonly BaseZoneBehavior Replacement;

            public DelayedZoneReplacement(HoldoutZoneType zoneType, BaseZoneBehavior original, BaseZoneBehavior replacement)
            {
                ReplacedZoneType = zoneType;
                Original = original;
                Replacement = replacement;
            }

            void apply(FogDamageController fogDamageController)
            {
                fogDamageController.RemoveSafeZone(Original);
                if (Original)
                    Destroy(Original);

                fogDamageController.AddSafeZone(Replacement);
            }

            static bool _isWaitingForZoneReplacementsDelay;
            static readonly List<DelayedZoneReplacement> _delayedZoneReplacements = new List<DelayedZoneReplacement>();

            public static void ApplyDelayedZoneReplacement(BaseZoneBehavior original, BaseZoneBehavior replacement, HoldoutZoneType replacedZoneType)
            {
                _delayedZoneReplacements.Add(new DelayedZoneReplacement(replacedZoneType, original, replacement));

                if (!_isWaitingForZoneReplacementsDelay)
                {
                    static IEnumerator waitThenApplyReplacements()
                    {
                        _isWaitingForZoneReplacementsDelay = true;

                        yield return new WaitForEndOfFrame();

                        _isWaitingForZoneReplacementsDelay = false;

                        foreach (DelayedZoneReplacement delayedReplacement in _delayedZoneReplacements)
                        {
                            FogDamageController fogDamageController = getFogDamageController(delayedReplacement.ReplacedZoneType, true);
                            if (fogDamageController)
                            {
                                delayedReplacement.apply(fogDamageController);
                            }
                        }

                        _delayedZoneReplacements.Clear();
                    }

                    Main.Instance.StartCoroutine(waitThenApplyReplacements());
                }
            }
        }

        static FogDamageController getFogDamageController(HoldoutZoneType replacementZoneType, bool createIfMissing)
        {
            if (_cachedFogController)
                return _cachedFogController;

            FogDamageController result = GameObject.FindObjectOfType<FogDamageController>();
            if (!result && createIfMissing)
            {
#if DEBUG
                Log.Debug($"Creating fog damage controller for {replacementZoneType}");
#endif

                switch (replacementZoneType)
                {
                    case HoldoutZoneType.InfiniteTowerSafeWard:
                    case HoldoutZoneType.NullSafeWard:
                        GameObject fogControllerObject = new GameObject($"{Main.PluginGUID}-{replacementZoneType}-FogController");

                        TeamFilter teamFilter = fogControllerObject.AddComponent<TeamFilter>();
                        teamFilter.teamIndex = TeamIndex.Neutral;

                        result = fogControllerObject.AddComponent<FogDamageController>();
                        result.damageTimer = 2f / 15f;
                        result.dangerBuffDef = bdVoidFogMild;
                        result.dangerBuffDuration = 0.4f;
                        result.dictionaryValidationTimer = 2f + (1f / 3f);
                        result.healthFractionPerSecond = 0.05f;
                        result.healthFractionRampCoefficientPerSecond = 0.1f;
                        result.initialSafeZones = Array.Empty<BaseZoneBehavior>();
                        result.invertTeamFilter = true;
                        result.safeZones = new List<IZone>();
                        result.teamFilter = teamFilter;
                        result.tickPeriodSeconds = 0.2f;

                        fogControllerObject.SetActive(false);
                        break;
                }
            }

            return _cachedFogController = result;
        }

        static BaseZoneBehavior getNonHoldoutZoneBehavior(HoldoutZoneController obj)
        {
            BaseZoneBehavior[] baseZoneBehaviors = obj.GetComponents<BaseZoneBehavior>();

            BaseZoneBehavior result = null;

            foreach (BaseZoneBehavior zone in baseZoneBehaviors)
            {
                if (zone != obj)
                {
                    if (result)
                    {
                        Log.Warning($"{nameof(HoldoutZoneController)} {obj.name} has multiple zone behaviors, previous={result.GetType().FullName}, current={zone.GetType().FullName}");
                    }

                    result = zone;
                }
            }

            return result;
        }

#region Patching
        public static readonly MethodInfo SetSphereZoneRadius_MI = SymbolExtensions.GetMethodInfo(() => setSphereZoneRadius(default, default, default));
        static void setSphereZoneRadius(SphereZone sphereZone, float radius, EntityState instance)
        {
            const string LOG_PREFIX = $"{nameof(RandomizedHoldoutZoneController)}.{nameof(setSphereZoneRadius)}";

            if (sphereZone)
            {
                sphereZone.Networkradius = radius;
                return;
            }
            else
            {
                RandomizedHoldoutZoneController randomizedHoldoutZoneController = instance.GetComponent<RandomizedHoldoutZoneController>();
                if (randomizedHoldoutZoneController)
                {
                    BaseZoneBehavior nonHoldoutZone = randomizedHoldoutZoneController.NonHoldoutZone;
                    if (nonHoldoutZone)
                    {
                        if (nonHoldoutZone is VerticalTubeZone tubeZone)
                        {
                            tubeZone.Networkradius = radius;
                        }
                        else
                        {
                            Log.Warning($"{LOG_PREFIX}: zone type {nonHoldoutZone.GetType()} is not implemented ({instance.outer})");
                        }

                        return;
                    }
                }
            }

            Log.Warning($"{LOG_PREFIX}: {instance.outer} has no zone");
        }

        public static readonly MethodInfo getZone_MI = SymbolExtensions.GetMethodInfo(() => getZone(default, default));
        static BaseZoneBehavior getZone(SphereZone sphereZone, EntityState instance)
        {
            const string LOG_PREFIX = $"{nameof(RandomizedHoldoutZoneController)}.{nameof(getZone)}";

            if (sphereZone)
                return sphereZone;

            RandomizedHoldoutZoneController randomizedHoldoutZoneController = instance.GetComponent<RandomizedHoldoutZoneController>();
            if (randomizedHoldoutZoneController)
            {
                BaseZoneBehavior nonHoldoutZone = randomizedHoldoutZoneController.NonHoldoutZone;
                if (nonHoldoutZone)
                    return nonHoldoutZone;
            }

            Log.Warning($"{LOG_PREFIX}: No zone type could be found for {instance.outer.name}");

            return null;
        }
#endregion
    }
}
#endif