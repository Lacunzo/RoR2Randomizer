using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
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
                BuffCatalog.FindBuffIndex("bdHiddenInvincibility"),
                BuffCatalog.FindBuffIndex("bdImmune"),
                BuffCatalog.FindBuffIndex("bdBearVoidReady")
            };
        });

        static ReplacementDictionary<BuffIndex> _buffReplacements;

        protected override void Awake()
        {
            base.Awake();

            Run.onRunStartGlobal += runStarted;
            Run.onRunDestroyGlobal += runEnd;
        }

        void OnDestroy()
        {
            Run.onRunStartGlobal -= runStarted;
            Run.onRunDestroyGlobal -= runEnd;
        }

        static void runStarted(Run instance)
        {
            if (ConfigManager.BuffRandomizer.Enabled && NetworkServer.active)
            {
                IEnumerable<BuffDef> buffsToRandomize = BuffCatalog.buffDefs.Where(b => b && b.buffIndex != BuffIndex.None && 
                   (!ConfigManager.BuffRandomizer.ExcludeInvincibility || Array.IndexOf(_invincibilityBuffs.Get, b.buffIndex) == -1));

#if DEBUG
                foreach (BuffDef excludedBuff in BuffCatalog.buffDefs.Except(buffsToRandomize))
                {
                    Log.Debug($"Buff randomizer: Excluded buff: {excludedBuff}");
                }
#endif

                _buffReplacements = ReplacementDictionary<BuffIndex>.CreateFrom<BuffDef>(buffsToRandomize, b => b.buffIndex, (key, value) =>
                {
                    if (!ConfigManager.BuffRandomizer.MixBuffsAndDebuffs && key.isDebuff != value.isDebuff)
                    {
#if DEBUG
                        Log.Debug($"{nameof(BuffRandomizerController)}: Not allowing replacement {key.name}->{value.name}, mixing buffs and debuffs is not enabled.");
#endif

                        return false;
                    }

                    return true;
                });

#if DEBUG
                foreach (KeyValuePair<BuffIndex, BuffIndex> pair in _buffReplacements)
                {
                    Log.Debug($"Replaced buff: {BuffCatalog.GetBuffDef(pair.Key)}->{BuffCatalog.GetBuffDef(pair.Value)}");
                }
#endif
            }
        }

        static void runEnd(Run instance)
        {
            _buffReplacements = null;
        }

        public static void TryReplaceBuffIndex(ref BuffIndex index)
        {
            if (NetworkServer.active && ConfigManager.BuffRandomizer.Enabled && _buffReplacements != null && _buffReplacements.TryGetReplacement(index, out BuffIndex replacement))
            {
                index = replacement;
            }
        }
    }
}
