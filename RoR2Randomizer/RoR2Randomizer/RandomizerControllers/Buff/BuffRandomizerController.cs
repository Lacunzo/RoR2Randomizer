using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Buff
{
    [RandomizerController]
    public class BuffRandomizerController : MonoBehaviour
    {
        public static bool IsActive => NetworkServer.active && ConfigManager.BuffRandomizer.Enabled && _buffReplacements.HasValue;

        static readonly InitializeOnAccess<Dictionary<BuffIndex, DotController.DotIndex>> _buffToDotIndex = new InitializeOnAccess<Dictionary<BuffIndex, DotController.DotIndex>>(() =>
        {
            Dictionary<BuffIndex, DotController.DotIndex> dict = new Dictionary<BuffIndex, DotController.DotIndex>();

            for (int i = 0; i < DotController.dotDefs.Length; i++)
            {
                DotController.DotDef dotDef = DotController.dotDefs[i];
                if (dotDef != null)
                {
                    BuffDef buffDef = dotDef.associatedBuff;
                    if (buffDef)
                    {
                        BuffIndex buffIndex = buffDef.buffIndex;
                        if (!dict.ContainsKey(buffIndex))
                        {
                            dict[buffIndex] = (DotController.DotIndex)i;
                        }
                    }
                }
            }

            return dict;
        });

        static readonly InitializeOnAccess<BuffIndex[]> _invincibilityBuffs = new InitializeOnAccess<BuffIndex[]>(() =>
        {
            return new BuffIndex[]
            {
                BuffCatalog.FindBuffIndex("bdBodyArmor"),
                BuffCatalog.FindBuffIndex("bdGoldEmpowered"),
                BuffCatalog.FindBuffIndex("bdHiddenInvincibility"),
                BuffCatalog.FindBuffIndex("bdImmune"),
                BuffCatalog.FindBuffIndex("bdIntangible"),
                BuffCatalog.FindBuffIndex("bdBearVoidReady")
            };
        });

        static readonly RunSpecific<ReplacementDictionary<BuffIndex>> _buffReplacements = new RunSpecific<ReplacementDictionary<BuffIndex>>((out ReplacementDictionary<BuffIndex> result) =>
        {
            if (ConfigManager.BuffRandomizer.Enabled && NetworkServer.active)
            {
                IEnumerable<BuffDef> buffsToRandomize = BuffCatalog.buffDefs.Where(b => b && b.buffIndex != BuffIndex.None &&
                   (!ConfigManager.BuffRandomizer.ExcludeInvincibility || Array.IndexOf(_invincibilityBuffs.Get, b.buffIndex) == -1));

#if DEBUG
                foreach (BuffDef excludedBuff in BuffCatalog.buffDefs.Except(buffsToRandomize))
                {
                    Log.Debug($"Buff randomizer: Excluded buff: {toLogString(excludedBuff)}");
                }
#endif

                result = ReplacementDictionary<BuffIndex>.CreateFrom<BuffDef>(buffsToRandomize, b => b.buffIndex, (key, value) =>
                {
                    if (!ConfigManager.BuffRandomizer.MixBuffsAndDebuffs && key.isDebuff != value.isDebuff)
                    {
#if DEBUG
                        Log.Debug($"{nameof(BuffRandomizerController)}: Not allowing replacement {toLogString(key)}->{toLogString(value)}, mixing buffs and debuffs is not enabled.");
#endif

                        return false;
                    }

                    return true;
                });

#if DEBUG
                foreach (KeyValuePair<BuffIndex, BuffIndex> pair in result)
                {
                    Log.Debug($"Replaced buff: {toLogString(pair.Key)} -> {toLogString(pair.Value)}");
                }
#endif

                return true;
            }

            result = default;
            return false;
        });

#if DEBUG
        static string toLogString(BuffIndex buff)
        {
            return toLogString(BuffCatalog.GetBuffDef(buff));
        }

        static string toLogString(BuffDef buff)
        {
            if (!buff || buff == null)
                return "null";

            return $"{buff.name} ({(int)buff.buffIndex})";
        }
#endif

#if DEBUG
        static BuffIndex _debugIndex;

        void Update()
        {
            if (ConfigManager.BuffRandomizer.Enabled && ConfigManager.BuffRandomizer.BuffDebugMode == DebugMode.Manual)
            {
                bool changed;
                if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_debugIndex < 0)
                        _debugIndex = (BuffIndex)(BuffCatalog.buffCount - 1);

                    changed = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (++_debugIndex >= (BuffIndex)BuffCatalog.buffCount)
                        _debugIndex = 0;

                    changed = true;
                }
                else
                {
                    changed = false;
                }

                if (changed)
                {
                    Log.Debug($"Buff Randomizer debug buff {toLogString(_debugIndex)}");
                }
            }
        }

        static bool tryGetDebugOverrideReplacement(out BuffIndex replacement)
        {
            switch (ConfigManager.BuffRandomizer.BuffDebugMode.Entry.Value)
            {
                case DebugMode.Manual:
                    replacement = _debugIndex;
                    return true;
                case DebugMode.Forced:
                    replacement = BuffCatalog.FindBuffIndex(ConfigManager.BuffRandomizer.ForcedBuffName);
                    return replacement != BuffIndex.None;
            }

            replacement = BuffIndex.None;
            return false;
        }
#endif

        void OnDestroy()
        {
            _buffReplacements.Dispose();
        }

        public static bool TryReplaceBuffIndex(ref BuffIndex index)
        {
            if (IsActive)
            {
                BuffIndex replacement;
                if (
#if DEBUG
                    tryGetDebugOverrideReplacement(out replacement) ||
#endif
                    _buffReplacements.Value.TryGetReplacement(index, out replacement))
                {
#if DEBUG
                    Log.Debug($"Buff Randomizer: Replaced {toLogString(index)} -> {toLogString(replacement)}");
#endif

                    index = replacement;
                    return true;
                }
            }

            return false;
        }

        public static bool TryReplaceBuffIndex(ref int index)
        {
            BuffIndex buffIndex = (BuffIndex)index;
            bool result = TryReplaceBuffIndex(ref buffIndex);
            index = (int)buffIndex;

            return result;
        }

        public static bool TryGetDotIndex(BuffIndex buff, out DotController.DotIndex dot)
        {
            return _buffToDotIndex.Get.TryGetValue(buff, out dot);
        }

        public static bool TryGetBuffIndex(DotController.DotIndex dot, out BuffIndex buff)
        {
            DotController.DotDef dotDef = DotController.GetDotDef(dot);
            if (dotDef != null)
            {
                BuffDef buffDef = dotDef.associatedBuff;
                if (buffDef)
                {
                    buff = buffDef.buffIndex;
                    return true;
                }
            }

            buff = BuffIndex.None;
            return false;
        }
    }
}
