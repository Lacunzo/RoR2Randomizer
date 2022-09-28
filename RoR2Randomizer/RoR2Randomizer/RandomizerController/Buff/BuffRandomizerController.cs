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

namespace RoR2Randomizer.RandomizerController.Buff
{
    public class BuffRandomizerController : Singleton<BuffRandomizerController>
    {
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
                _buffReplacements = ReplacementDictionary<BuffIndex>.CreateFrom<BuffDef>(BuffCatalog.buffDefs.Where(b => b && b.buffIndex != BuffIndex.None), b => b.buffIndex, (key, value) =>
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
