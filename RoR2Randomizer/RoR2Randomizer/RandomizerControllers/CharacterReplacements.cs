using HG;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.CharacterReplacements;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers
{
#if DEBUG
    public enum DebugMode : byte
    {
        None,
        Manual,
        Forced
    }
#endif

    public static class CharacterReplacements
    {
#if DEBUG
        public static DebugMode DebugMode => ConfigManager.Debug.CharacterDebugMode;
#endif

        public static ReadOnlyArray<EquipmentIndex> AvailableDroneEquipments;

        static int[] _masterIndicesToRandomize;

        [SystemInitializer(typeof(EquipmentCatalog), typeof(MasterCatalog))]
        static void Init()
        {
            AvailableDroneEquipments = EquipmentCatalog.equipmentDefs.Where(eq => eq && (eq.canDrop || eq.name == "BossHunterConsumed"))
                                                                     .Select(e => e.equipmentIndex)
                                                                     .ToArray();

            _masterIndicesToRandomize = MasterCatalog.masterPrefabs.Where(master =>
            {
                const string LOG_PREFIX = $"{nameof(CharacterReplacements)}.{nameof(Init)} ";

                if (!master)
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"excluding null master");
#endif

                    return false;
                }

                if (!master.TryGetComponent<CharacterMaster>(out CharacterMaster masterComponent))
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"excluding master {master.name}: no {nameof(CharacterMaster)} component");
#endif

                    return false;
                }

                if (!masterComponent.bodyPrefab)
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"excluding master {master.name}: no {nameof(CharacterMaster.bodyPrefab)}");
#endif

                    return false;
                }

                if (!masterComponent.bodyPrefab.TryGetComponent<CharacterBody>(out CharacterBody body))
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"excluding master {master.name}: no {nameof(CharacterMaster.bodyPrefab)} {nameof(CharacterBody)} component");
#endif

                    return false;
                }

                Transform modelTransform;
                if (!body.TryGetComponent<ModelLocator>(out ModelLocator modelLocator) || !(modelTransform = modelLocator.modelTransform))
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"excluding master {master.name}: no model");
#endif

                    return false;
                }

                if (modelTransform.childCount == 0)
                {
#if DEBUG
                    Log.Debug(LOG_PREFIX + $"excluding master {master.name}: empty model");
#endif

                    return false;
                }

                switch (master.name)
                {
                    case "AncientWispMaster": // Does nothing
                    case "ArtifactShellMaster": // No model, does not attack, cannot be damaged
                    case "ClaymanMaster": // No hitboxes
                    case "EngiBeamTurretMaster": // Seems to ignore the player
                    case "MinorConstructAttachableMaster": // Instantly dies
                    case "VoidRaidCrabJointMaster": // Balls
                    case "VoidRaidCrabMaster": // Beta voidling, half invisible
#if DEBUG
                        Log.Debug(LOG_PREFIX + $"excluding master {master.name}: blacklist");
#endif
                        return false;
                }

                return true;
            }).Distinct().Select(go => (int)MasterCatalog.FindMasterIndex(go)).ToArray();
        }

        static readonly RunSpecific<bool> _hasReceivedMasterIndexReplacementsFromServer = new RunSpecific<bool>(1);
        static readonly RunSpecific<IndexReplacementsCollection> _masterIndexReplacements = new RunSpecific<IndexReplacementsCollection>((out IndexReplacementsCollection result) =>
        {
            if (NetworkServer.active)
            {
                result = new IndexReplacementsCollection(ReplacementDictionary<int>.CreateFrom(_masterIndicesToRandomize), MasterCatalog.masterPrefabs.Length);

                new SyncCharacterMasterReplacements(result).Send(NetworkDestination.Clients);

                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }, 1);

        public static bool IsEnabled => NetworkServer.active || (NetworkClient.active && _hasReceivedMasterIndexReplacementsFromServer);

        static void onMasterReplacementsReceivedFromServer(IndexReplacementsCollection masterIndexReplacements)
        {
#if DEBUG
            Log.Debug("Received master index replacements from server");
#endif

            _masterIndexReplacements.Value = masterIndexReplacements;
            _hasReceivedMasterIndexReplacementsFromServer.Value = true;
        }

        public static void Initialize()
        {
            SyncCharacterMasterReplacements.OnReceive += onMasterReplacementsReceivedFromServer;

#if DEBUG
            RoR2Application.onFixedUpdate += Update;
#endif
        }

        public static void Uninitialize()
        {
            _masterIndexReplacements.Dispose();
            _hasReceivedMasterIndexReplacementsFromServer.Dispose();

            SyncCharacterMasterReplacements.OnReceive -= onMasterReplacementsReceivedFromServer;

#if DEBUG
            RoR2Application.onFixedUpdate -= Update;
#endif
        }

#if DEBUG
        static int _forcedMasterIndex;
        static void Update()
        {
            if (DebugMode == DebugMode.Manual)
            {
                bool changed = false;
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (++_forcedMasterIndex >= _masterIndicesToRandomize.Length)
                        _forcedMasterIndex = 0;

                    changed = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_forcedMasterIndex < 0)
                        _forcedMasterIndex = _masterIndicesToRandomize.Length - 1;

                    changed = true;
                }

                if (changed)
                {
                    Log.Debug($"Character master: {MasterCatalog.GetMasterPrefab((MasterCatalog.MasterIndex)_masterIndicesToRandomize[_forcedMasterIndex])?.name} ({_forcedMasterIndex})");
                }
            }
        }
#endif

        public static GameObject GetReplacementMasterPrefab(string originalMasterName)
        {
            MasterCatalog.MasterIndex replacementIndex = GetReplacementForMasterIndex(MasterCatalog.FindMasterIndex(originalMasterName));
            if (replacementIndex.isValid)
            {
                GameObject replacement = MasterCatalog.GetMasterPrefab(replacementIndex);

#if DEBUG
                Log.Debug($"{nameof(CharacterReplacements)}: Replaced {originalMasterName} with {replacement}");
#endif

                return replacement;
            }
            else
            {
                return null;
            }
        }

        public static bool TryReplaceMasterPrefab(ref GameObject prefab)
        {
            GameObject replacement = GetReplacementMasterPrefab(prefab.name);
            if (replacement)
            {
                prefab = replacement;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static MasterCatalog.MasterIndex GetReplacementForMasterIndex(MasterCatalog.MasterIndex original)
        {
            if (original.isValid && IsEnabled)
            {
#if DEBUG
                if (NetworkServer.active)
                {
                    if (DebugMode == DebugMode.Manual)
                    {
                        int masterIndex = ArrayUtils.GetSafe(_masterIndicesToRandomize, _forcedMasterIndex, -1);
                        if (masterIndex != -1)
                        {
                            return (MasterCatalog.MasterIndex)masterIndex;
                        }
                    }

                    if (DebugMode == DebugMode.Forced)
                    {
                        return MasterCatalog.FindMasterIndex(ConfigManager.Debug.ForcedMasterName);
                    }
                }
#endif

                if (_masterIndexReplacements.HasValue && _masterIndexReplacements.Value.TryGetReplacement((int)original, out int replacementIndex))
                {
                    return (MasterCatalog.MasterIndex)replacementIndex;
                }
            }

            return MasterCatalog.MasterIndex.none;
        }

        public static MasterCatalog.MasterIndex GetOriginalMasterIndex(MasterCatalog.MasterIndex replacement)
        {
            if (replacement.isValid && IsEnabled)
            {
                if (_masterIndexReplacements.HasValue && _masterIndexReplacements.Value.TryGetOriginal((int)replacement, out int originalIndex))
                {
                    return (MasterCatalog.MasterIndex)originalIndex;
                }
            }

            return MasterCatalog.MasterIndex.none;
        }

        public static ILContext.Manipulator FixMasterIndexReferences(Func<bool> patchEnabled, params string[] names)
        {
            return il =>
            {
                ILCursor c = new ILCursor(il);

                MemberReference member = null;
                while (c.TryGotoNext(x => x.MatchGetMemberValue(out member)))
                {
                    c.Index++;

                    if (member != null && Array.IndexOf(names, member.Name) != -1)
                    {
                        TypeReference valueType = null;
                        if (member is MethodReference method)
                        {
                            valueType = method.ReturnType;
                        }
                        else if (member is FieldReference field)
                        {
                            valueType = field.FieldType;
                        }
                        else
                        {
                            Log.Warning($"{nameof(CharacterReplacements)}.{nameof(FixMasterIndexReferences)} unimplemented member type of {member.GetType().FullName}");
                        }

                        if (valueType != null)
                        {
                            MasterCatalog.MasterIndex replacefunc(MasterCatalog.MasterIndex original)
                            {
                                if (patchEnabled == null || patchEnabled())
                                {
                                    MasterCatalog.MasterIndex replacement = GetReplacementForMasterIndex(original);
                                    if (replacement.isValid)
                                    {
                                        return replacement;
                                    }
                                }

                                return original;
                            }

                            if (valueType.Is(typeof(MasterCatalog.MasterIndex)))
                            {
                                c.EmitDelegate(replacefunc);
                            }
                            else if (valueType.Is(typeof(int)))
                            {
                                c.EmitDelegate((int original) =>
                                {
                                    return (int)replacefunc((MasterCatalog.MasterIndex)original);
                                });
                            }
                            else
                            {
                                Log.Warning($"{nameof(CharacterReplacements)}.{nameof(FixMasterIndexReferences)} unimplemented value type of {valueType.FullName}");
                            }
                        }
                    }
                }
            };
        }
    }
}
