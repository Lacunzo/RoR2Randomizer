using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.EffectRandomizer;
using RoR2Randomizer.Networking.Generic;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.Effect
{
    [RandomizerController]
    public sealed class EffectRandomizerController : BaseRandomizerController
    {
        static readonly RunSpecific<bool> _hasRecievedEffectReplacementsFromServer = new RunSpecific<bool>();
        static readonly RunSpecific<IndexReplacementsCollection> _effectReplacements = new RunSpecific<IndexReplacementsCollection>((out IndexReplacementsCollection result) =>
        {
            if (NetworkServer.active && ConfigManager.Misc.EffectRandomizerEnabled)
            {
                result = new IndexReplacementsCollection(ReplacementDictionary<int>.CreateFrom(Enumerable.Range(0, EffectCatalog.effectCount)), EffectCatalog.effectCount);
                return true;
            }

            result = default;
            return false;
        });

        static bool shouldBeEnabled => ((NetworkServer.active && ConfigManager.Misc.EffectRandomizerEnabled) || (NetworkClient.active && _hasRecievedEffectReplacementsFromServer)) && _effectReplacements.HasValue;

        public override bool IsRandomizerEnabled => shouldBeEnabled;

        protected override bool isNetworked => true;

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
#if DEBUG
            Log.Debug($"Sending {nameof(SyncEffectReplacements)} to clients");
#endif

            yield return new SyncEffectReplacements(_effectReplacements);
        }

        void setEffectReplacementsFromServerEvent(IndexReplacementsCollection replacements)
        {
            if (NetworkServer.active || !NetworkClient.active)
                return;
            
            _effectReplacements.Value = replacements;
            _hasRecievedEffectReplacementsFromServer.Value = _effectReplacements.HasValue;
        }

        protected override void Awake()
        {
            base.Awake();

            SyncEffectReplacements.OnReceive += setEffectReplacementsFromServerEvent;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _hasRecievedEffectReplacementsFromServer.Dispose();
            _effectReplacements.Dispose();

            SyncEffectReplacements.OnReceive -= setEffectReplacementsFromServerEvent;
        }

        public static void TryReplaceEffectIndex(ref EffectIndex index)
        {
            if (shouldBeEnabled && _effectReplacements.Value.TryGetReplacement(index, out EffectIndex replacement))
            {
#if DEBUG
                Log.Debug($"Effect randomizer: replaced effect {EffectCatalog.GetEffectDef(index).prefabName} ({(int)index}) -> {EffectCatalog.GetEffectDef(replacement).prefabName} ({(int)replacement})");
                
#endif
                index = replacement;
            }
        }
    }
}
