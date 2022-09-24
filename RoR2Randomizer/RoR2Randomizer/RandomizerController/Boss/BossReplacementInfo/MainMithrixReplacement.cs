using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System.Collections;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public sealed class MainMithrixReplacement : BaseMithrixReplacement
    {
        public bool IsHurt;

        protected override BossReplacementType ReplacementType => IsHurt ? BossReplacementType.MithrixHurt : BossReplacementType.MithrixNormal;

        protected override IEnumerator initializeClient()
        {
            yield return base.initializeClient();

            CoroutineOut<CharacterBody> bodyOut = new CoroutineOut<CharacterBody>();
            yield return getBody(bodyOut);

            CharacterBody body = bodyOut.Result;

#if DEBUG
            Log.Debug($"{nameof(MainMithrixReplacement)} {nameof(initializeClient)}: IsHurt={IsHurt}, _master={_master}, body={(bool)body}");
#endif

            if (body)
            {
#if DEBUG
                Log.Debug($"{nameof(MainMithrixReplacement)} {nameof(initializeClient)}: body.subtitleNameToken={body.subtitleNameToken}");
#endif
                if (string.IsNullOrEmpty(body.subtitleNameToken))
                {
                    body.subtitleNameToken = "BROTHER_BODY_SUBTITLE";
                }
            }
        }

        protected override IEnumerator initializeServer()
        {
            yield return base.initializeServer();

            CharacterBody body = _cachedBody;
            if (!body)
            {
                CoroutineOut<CharacterBody> bodyOut = new CoroutineOut<CharacterBody>();
                yield return getBody(bodyOut);

                body = bodyOut.Result;
            }

            if (body && IsHurt)
            {
                // Prevent low-health replacements from instantly dying
                body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, SpellChannelEnterState.duration);

                EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                if (bodyState)
                {
                    if (bodyState.state != null)
                    {
                        bodyState.SetState(EntityStateCatalog.InstantiateState(BossRandomizerController.Mithrix.MithrixHurtInitialState));
                    }
                    else
                    {
                        bodyState.initialStateType = BossRandomizerController.Mithrix.MithrixHurtInitialState;
                    }
                }
                else
                {
                    Log.Warning($"Body entityState for {body.GetDisplayName()} could not be found!");
                }

                if (!body.GetComponent<ReturnStolenItemsOnGettingHit>())
                {
                    AddReturnStolenItemsOnGettingHit(body.gameObject, body.healthComponent);
                }
            }
        }

        public static ReturnStolenItemsOnGettingHit AddReturnStolenItemsOnGettingHit(GameObject bodyObj, HealthComponent healthComponent)
        {
            ReturnStolenItemsOnGettingHit newReturnItems = bodyObj.AddComponent<ReturnStolenItemsOnGettingHit>();

            newReturnItems.healthComponent = healthComponent;

            ReturnStolenItemsOnGettingHit mithrixReturnItems = BossRandomizerController.Mithrix.MithrixReturnItemsComponent.Get;
            newReturnItems.minPercentagePerItem = mithrixReturnItems.minPercentagePerItem;
            newReturnItems.maxPercentagePerItem = mithrixReturnItems.maxPercentagePerItem;

            newReturnItems.initialPercentageToFirstItem = mithrixReturnItems.initialPercentageToFirstItem;

            // Re-run awake
            newReturnItems.Awake();

            if (healthComponent)
            {
                MiscUtils.AddItem(ref healthComponent.onTakeDamageReceivers, newReturnItems);
            }

            return newReturnItems;
        }
    }
}
