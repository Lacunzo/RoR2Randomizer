using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Buff
{
    public class BuffRandomizerController : Singleton<BuffRandomizerController>
    {
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

        void OnDestroy()
        {
            _buffReplacements.Dispose();
        }

        public static void TryReplaceBuffIndex(ref BuffIndex index)
        {
            if (NetworkServer.active && ConfigManager.BuffRandomizer.Enabled && _buffReplacements.HasValue && _buffReplacements.Value.TryGetReplacement(index, out BuffIndex replacement))
            {
#if DEBUG
                Log.Debug($"Buff Randomizer: Replaced {toLogString(index)} -> {toLogString(replacement)}");
#endif

                index = replacement;
            }
        }

        public static void TryReplaceBuffIndex(ref int index)
        {
            BuffIndex buffIndex = (BuffIndex)index;
            TryReplaceBuffIndex(ref buffIndex);
            index = (int)buffIndex;
        }
    }
}
