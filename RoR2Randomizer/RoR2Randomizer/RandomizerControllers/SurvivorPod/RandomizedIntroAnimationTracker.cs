using EntityStates;
using RoR2;
using RoR2Randomizer.BodyAnimationMirroring;
using System;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.SurvivorPod
{
    public class RandomizedIntroAnimationTracker : MonoBehaviour
    {
        public SerializableEntityStateType IntroState;
        public bool HasEnteredIntroState { get; private set; }

        public CharacterAnimationMirrorOwner AnimationMirrorController;

        EntityStateMachine _bodyEntityStateMachine;

        void Start()
        {
            const string LOG_PREFIX = $"{nameof(RandomizedIntroAnimationTracker)}.{nameof(Start)} ";

            const string BODY_STATE_MACHINE_NAME = "Body";
            _bodyEntityStateMachine = EntityStateMachine.FindByCustomName(gameObject, BODY_STATE_MACHINE_NAME);
            if (_bodyEntityStateMachine)
            {
                checkHasEnteredIntroState();
            }
            else
            {
                Log.Warning(LOG_PREFIX + $"{name} has no state machine with the name {BODY_STATE_MACHINE_NAME}");
            }
        }

        void Update()
        {
            if (!HasEnteredIntroState && _bodyEntityStateMachine)
            {
                checkHasEnteredIntroState();
            }
        }

        void checkHasEnteredIntroState()
        {
            EntityState currentState = _bodyEntityStateMachine.state;
            if (currentState != null && currentState.GetType() == IntroState.stateType)
            {
                HasEnteredIntroState = true;
            }
        }

        public void OnNextStateSetToMain()
        {
            if (HasEnteredIntroState)
            {
                if (AnimationMirrorController)
                {
                    AnimationMirrorController.StopMirroring(0f);
                }

                Destroy(this);
            }
        }
    }
}
