using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public abstract class BaseBossReplacement : CharacterReplacementInfo
    {
        protected abstract BossReplacementType replacementType { get; }

        protected virtual bool replaceBossDropEvenIfExisting => false;

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

        protected override void initializeServer()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeServer)}");
#endif

            new SyncBossReplacementCharacter(_master.gameObject, replacementType).Send(NetworkDestination.Clients);

#if DEBUG
            Log.Debug($"Sent {nameof(SyncBossReplacementCharacter)} to clients");
#endif
        }

        protected void setBodySubtitleIfNull(string subtitleToken)
        {
            if (string.IsNullOrEmpty(_body.subtitleNameToken))
            {
                setBodySubtitle(subtitleToken);
            }
        }

        protected void setBodySubtitle(string subtitleToken)
        {
            if (_body && _body.subtitleNameToken != subtitleToken)
            {
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
            BossGroup[] bossGroups = GameObject.FindObjectsOfType<BossGroup>();
            foreach (BossGroup group in bossGroups)
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
