using EntityStates;
using RoR2;
using RoR2Randomizer.BodyAnimationMirroring;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.SurvivorPod
{
    [RequireComponent(typeof(CharacterBody))]
    public class MirroredIntroAnimationTracker : NetworkBehaviour
    {
        public SerializableEntityStateType IntroState;

        CharacterBody _body;

        [SyncVar(hook = nameof(onMirrorBodyIndexModified))]
        int _mirrorBodyIndex = (int)BodyIndex.None;
        const uint _mirrorBodyIndex_DIRTY_BIT = 1U << 0;

        public BodyIndex mirrorBodyIndex => (BodyIndex)_mirrorBodyIndex;

        public BodyIndex Network_mirrorBodyIndex
        {
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && !syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    onMirrorBodyIndexModified(value);
                    syncVarHookGuard = false;
                }

                SetSyncVar((int)value, ref _mirrorBodyIndex, _mirrorBodyIndex_DIRTY_BIT);
            }
        }

        enum State : byte
        {
            WaitingForIntro,
            HasEnteredIntro,
            HasEnteredMain
        }

        [SyncVar(hook = nameof(onIntroStateModified))]
        byte _introStateByte = (byte)State.WaitingForIntro;
        const uint _introStateByte_DIRTY_BIT = 1U << 1;

        State introState => (State)_introStateByte;

        State Network_introState
        {
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && !syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    onIntroStateModified(value);
                    syncVarHookGuard = false;
                }

                SetSyncVar((byte)value, ref _introStateByte, _introStateByte_DIRTY_BIT);
            }
        }

        [SyncVar]
        float _tweenTime;
        const uint _tweenTime_DIRTY_BIT = 1U << 2;

        public float Network_tweenTime
        {
            [param: In]
            set
            {
                SetSyncVar(value, ref _tweenTime, _tweenTime_DIRTY_BIT);
            }
        }

        public CharacterAnimationMirrorOwner AnimationMirrorController;

        EntityStateMachine _bodyEntityStateMachine;

        bool _isWaitingForModel;

        void Awake()
        {
            _body = GetComponent<CharacterBody>();

            const float LONG_TWEEN_TIME_CHANCE = 0.05f;
            if (RoR2Application.rng.nextNormalizedFloat <= LONG_TWEEN_TIME_CHANCE)
            {
                // For the funny
                Network_tweenTime = RoR2Application.rng.RangeFloat(3f, 20f);
#if DEBUG
                Log.Debug($"Long tween time: {_tweenTime}");
#endif
            }
            else
            {
                Network_tweenTime = 0.3f;
            }
        }

        void Start()
        {
            const string LOG_PREFIX = $"{nameof(MirroredIntroAnimationTracker)}.{nameof(Start)} ";

            const string BODY_STATE_MACHINE_NAME = "Body";
            _bodyEntityStateMachine = EntityStateMachine.FindByCustomName(gameObject, BODY_STATE_MACHINE_NAME);
            if (isServer)
            {
                if (_bodyEntityStateMachine)
                {
                    checkHasEnteredIntroState();
                }
                else
                {
                    Log.Warning(LOG_PREFIX + $"{name} has no state machine with the name {BODY_STATE_MACHINE_NAME}");
                }
            }
        }

        void Update()
        {
            if (!_bodyEntityStateMachine)
                return;

            if (isServer)
            {
                switch (introState)
                {
                    case State.WaitingForIntro:
                        checkHasEnteredIntroState();
                        break;
                    case State.HasEnteredIntro:
                        checkIsMainState();
                        break;
                }
            }
        }

        bool isInState(SerializableEntityStateType entityStateType)
        {
            if (!_bodyEntityStateMachine)
                return false;

            EntityState currentState = _bodyEntityStateMachine.state;
            return currentState != null && currentState.GetType() == entityStateType.stateType;
        }

        void checkHasEnteredIntroState()
        {
            if (isInState(IntroState))
            {
                Network_introState = State.HasEnteredIntro;
            }
        }

        void checkIsMainState()
        {
            if (isInState(_bodyEntityStateMachine.mainStateType))
            {
                Network_introState = State.HasEnteredMain;
            }
        }

        void onIntroStateModified(State newIntroState)
        {
            if (newIntroState >= State.HasEnteredMain)
            {
                if (AnimationMirrorController)
                {
                    AnimationMirrorController.StopMirroring(_tweenTime);
                }
            }
            else if (mirrorBodyIndex != BodyIndex.None)
            {
                updateMirrorController();
            }

            Network_introState = newIntroState;
        }

        void onMirrorBodyIndexModified(BodyIndex newBodyIndex)
        {
            Network_mirrorBodyIndex = newBodyIndex;
            updateMirrorController();
        }

        void updateMirrorController()
        {
            if (_isWaitingForModel || mirrorBodyIndex == BodyIndex.None)
                return;

            if (!AnimationMirrorController)
            {
                ModelLocator modelLocator = _body.modelLocator;

                void setupModel(Transform model)
                {
                    if (model)
                    {
                        AnimationMirrorController = CharacterAnimationMirrorOwner.SetupForModelTransform(model, mirrorBodyIndex);
                    }

                    if (modelLocator)
                    {
                        modelLocator.onModelChanged -= setupModel;
                    }

                    _isWaitingForModel = false;
                }

                Transform modelTransform = modelLocator.modelTransform;
                if (modelTransform)
                {
                    setupModel(modelTransform);
                }
                else
                {
                    _isWaitingForModel = true;
                    modelLocator.onModelChanged += setupModel;
                }
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                writer.WritePackedIndex32(_mirrorBodyIndex);
                writer.Write(_introStateByte);
                writer.Write(_tweenTime);
                return true;
            }

            uint dirtyBits = syncVarDirtyBits;
            writer.WritePackedUInt32(dirtyBits);

            bool anythingWritten = false;

            if ((dirtyBits & _mirrorBodyIndex_DIRTY_BIT) != 0U)
            {
                writer.WritePackedIndex32(_mirrorBodyIndex);
                anythingWritten = true;
            }

            if ((dirtyBits & _introStateByte_DIRTY_BIT) != 0U)
            {
                writer.Write(_introStateByte);
                anythingWritten = true;
            }

            if ((dirtyBits & _tweenTime_DIRTY_BIT) != 0U)
            {
                writer.Write(_tweenTime);
                anythingWritten = true;
            }

            return anythingWritten;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            const string LOG_PREFIX = $"{nameof(MirroredIntroAnimationTracker)}.{nameof(OnDeserialize)} ";

            if (initialState)
            {
                _mirrorBodyIndex = reader.ReadPackedIndex32();
                _introStateByte = reader.ReadByte();
                _tweenTime = reader.ReadSingle();

#if DEBUG
                Log.Debug(LOG_PREFIX + $"({nameof(initialState)}) received tween time {_tweenTime}");
#endif

                return;
            }

            uint dirtyBits = reader.ReadPackedUInt32();

            if ((dirtyBits & _mirrorBodyIndex_DIRTY_BIT) != 0U)
            {
                onMirrorBodyIndexModified((BodyIndex)reader.ReadPackedIndex32());
            }

            if ((dirtyBits & _introStateByte_DIRTY_BIT) != 0U)
            {
                onIntroStateModified((State)reader.ReadByte());
            }

            if ((dirtyBits & _tweenTime_DIRTY_BIT) != 0U)
            {
                _tweenTime = reader.ReadSingle();

#if DEBUG
                Log.Debug(LOG_PREFIX + $"received tween time {_tweenTime}");
#endif
            }
        }
    }
}
