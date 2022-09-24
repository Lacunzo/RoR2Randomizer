using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Voidling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public class VoidlingReplacement : BaseBossReplacement
    {
        public int Phase;

        protected override BossReplacementType ReplacementType => Phase switch
        {
            1 => BossReplacementType.VoidlingPhase1,
            2 => BossReplacementType.VoidlingPhase2,
            3 => BossReplacementType.VoidlingPhase3,
            _ => BossReplacementType.Invalid
        };

        protected override IEnumerator initializeClient()
        {
            yield return base.initializeClient();

            CharacterBody body = _cachedBody;
            if (!body)
            {
                CoroutineOut<CharacterBody> bodyOut = new CoroutineOut<CharacterBody>();
                yield return getBody(bodyOut);

                body = bodyOut.Result;
            }

            if (body)
            {
                if (string.IsNullOrEmpty(body.subtitleNameToken))
                {
                    body.subtitleNameToken = "VOIDRAIDCRAB_BODY_SUBTITLE";
                }

                const string BODY_STATE_MACHINE_NAME = "Body";

                CharacterDeathBehavior deathBehavior = body.gameObject.GetOrAddComponent<CharacterDeathBehavior>();
                if (!deathBehavior.deathStateMachine || deathBehavior.deathStateMachine.customName != BODY_STATE_MACHINE_NAME)
                {
                    EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(body.gameObject, BODY_STATE_MACHINE_NAME);
                    if (bodyState)
                    {
                        deathBehavior.deathStateMachine = bodyState;
                    }
                    else
                    {
                        Log.Warning($"Body entityState for {body.GetDisplayName()} could not be found!");
                    }
                }

                deathBehavior.deathState = Phase == VoidlingPhaseTracker.TotalNumPhases ? BossRandomizerController.Voidling.FinalDeathState
                                                                                        : BossRandomizerController.Voidling.EscapeDeathState;

                deathBehavior.idleStateMachine ??= Array.Empty<EntityStateMachine>();
            }
        }
    }
}
