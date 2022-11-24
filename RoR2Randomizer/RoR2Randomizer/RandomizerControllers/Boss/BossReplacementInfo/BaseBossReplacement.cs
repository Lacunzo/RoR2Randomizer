using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.Generic;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public abstract class BaseBossReplacement : CharacterReplacementInfo
    {
        protected abstract BossReplacementType replacementType { get; }

        protected virtual bool replaceBossDropEvenIfExisting => false;

        protected override bool isNetworked => true;

        protected virtual bool sendOriginalCharacterMasterIndex => false;

        protected override void bodyResolved()
        {
            base.bodyResolved();

            CharacterBody originalBodyprefab = originalBodyPrefab;
            if (originalBodyprefab)
            {
                if (originalBodyprefab.TryGetComponent<DeathRewards>(out DeathRewards prefabDeathRewards) && prefabDeathRewards.bossDropTable)
                {
                    DeathRewards deathRewards = _body.gameObject.GetOrAddComponent<DeathRewards>();
                    if (!deathRewards.bossDropTable || deathRewards.bossDropTable == null || replaceBossDropEvenIfExisting)
                    {
                        deathRewards.bossDropTable = prefabDeathRewards.bossDropTable;
                    }
                }

                HealthComponent healthComponent = _body.healthComponent;
                if (healthComponent && originalBodyprefab.TryGetComponent<HealthComponent>(out HealthComponent prefabHealthComponent))
                {
#if DEBUG
                    float oldValue = healthComponent.globalDeathEventChanceCoefficient;
#endif

                    healthComponent.globalDeathEventChanceCoefficient = prefabHealthComponent.globalDeathEventChanceCoefficient;

#if DEBUG
                    if (oldValue != healthComponent.globalDeathEventChanceCoefficient)
                    {
                        Log.Debug($"{nameof(BaseBossReplacement)}: overriding {nameof(HealthComponent.globalDeathEventChanceCoefficient)} for {replacementType} replacement {_master.name} ({oldValue} -> {healthComponent.globalDeathEventChanceCoefficient})");
                    }
#endif
                }
            }
        }

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
#if DEBUG
            Log.Debug($"Sending {nameof(SyncBossReplacementCharacter)} to clients");
#endif

            yield return new SyncBossReplacementCharacter(_master.gameObject, replacementType, sendOriginalCharacterMasterIndex ? originalMasterPrefab?.masterIndex : null);
        }

        public enum SetSubtitleMode : byte
        {
            OnlyIfExistingIsNull,
            DontOverrideIfBothNotNull,
            AlwaysOverride
        }

        protected void setBodySubtitle(string subtitleToken, SetSubtitleMode mode = SetSubtitleMode.OnlyIfExistingIsNull)
        {
            if (_body && _body.subtitleNameToken != subtitleToken)
            {
                if (mode == SetSubtitleMode.OnlyIfExistingIsNull)
                {
                    if (!string.IsNullOrEmpty(_body.subtitleNameToken))
                        return;
                }
                else if (mode == SetSubtitleMode.DontOverrideIfBothNotNull)
                {
                    if (!string.IsNullOrEmpty(_body.subtitleNameToken) && !string.IsNullOrEmpty(subtitleToken))
                        return;
                }

                _body.subtitleNameToken = subtitleToken;

                // Update BossGroup
                if (_master.isBoss)
                {
                    resetBossGroupSubtitle();
                }
            }
        }

        void resetBossGroupSubtitle()
        {
            foreach (BossGroup group in InstanceTracker.GetInstancesList<BossGroup>())
            {
                for (int i = 0; i < group.bossMemoryCount; i++)
                {
                    if (group.bossMemories[i].cachedMaster == _master)
                    {
                        // Force a refresh of the boss subtitle
                        group.bestObservedName = string.Empty;
                        group.bestObservedSubtitle = string.Empty;
                        return;
                    }
                }
            }
        }
    }
}
