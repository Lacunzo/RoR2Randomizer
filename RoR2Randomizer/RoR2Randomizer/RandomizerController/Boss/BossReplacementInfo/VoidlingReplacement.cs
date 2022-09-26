using EntityStates;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Patches.BossRandomizer.Voidling;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    // TODO: Add SphereZone to phase 3 replacements
    // UrchinTurretMaster (102) does not spawn gauntlet portal
    // Stopped at VoidInfestorMaster (108)
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

        protected override void bodyResolved()
        {
            base.bodyResolved();

            _body.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            if (string.IsNullOrEmpty(_body.subtitleNameToken))
            {
                setBodySubtitle("VOIDRAIDCRAB_BODY_SUBTITLE");
            }

            CharacterDeathBehavior deathBehavior = _body.gameObject.GetOrAddComponent<CharacterDeathBehavior>();
            if (!deathBehavior.deathStateMachine)
            {
                EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(_body.gameObject, "Body");
                if (bodyState)
                {
                    deathBehavior.deathStateMachine = bodyState;
                }
                else
                {
                    Log.Warning($"Body entityState for {_body.GetDisplayName()} could not be found!");
                }
            }

            SerializableEntityStateType voidlingDeathState = Phase == VoidlingPhaseTracker.TotalNumPhases ? BossRandomizerController.Voidling.FinalDeathState
                                                                                                          : BossRandomizerController.Voidling.EscapeDeathState;

            Type deathStateType = deathBehavior.deathState.stateType;
            if (deathStateType == null || deathStateType == BossRandomizerController.Voidling.EscapeDeathState.stateType) // Don't combine anything with the escape death state, since that always opens a portal to the next gauntlet, where there is nothing if it happens on phase 3
            {
                deathBehavior.deathState = voidlingDeathState;
            }
            else if (deathStateType != voidlingDeathState.stateType) // If the death state is already the one we want, nothing needs to be done
            {
                MultiEntityStateSubStatesData subStatesData = deathBehavior.gameObject.AddComponent<MultiEntityStateSubStatesData>();
                subStatesData.StateTypes = new SerializableEntityStateType[] { deathBehavior.deathState, voidlingDeathState };

                deathBehavior.deathState = MultiEntityState.SerializableStateType;
            }

            deathBehavior.idleStateMachine ??= Array.Empty<EntityStateMachine>();
        }
    }
}
