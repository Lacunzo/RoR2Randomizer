using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public sealed class MainMithrixReplacement : BaseMithrixReplacement
    {
        public bool IsHurt;

        protected override BossReplacementType replacementType => IsHurt ? BossReplacementType.MithrixHurt : BossReplacementType.MithrixNormal;

        protected override void bodyResolved()
        {
            base.bodyResolved();

#if DEBUG
            Log.Debug($"{nameof(MainMithrixReplacement)} {nameof(initializeClient)}: body.subtitleNameToken={_body.subtitleNameToken}");
#endif

            setBodySubtitleIfNull("BROTHER_BODY_SUBTITLE");
            
            if (NetworkServer.active)
            {
                if (IsHurt)
                {
                    // Prevent low-health replacements from instantly dying
                    _body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, SpellChannelEnterState.duration);

                    EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(_body.gameObject, "Body");
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
                        Log.Warning($"Body entityState for {_body.GetDisplayName()} could not be found!");
                    }

                    if (!_body.GetComponent<ReturnStolenItemsOnGettingHit>())
                    {
                        AddReturnStolenItemsOnGettingHit(_body.gameObject, _body.healthComponent);
                    }
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
