using EntityStates;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.CustomContent
{
    public sealed class MultiEntityState : EntityState
    {
        public class SubStatesData : MonoBehaviour
        {
            public SerializableEntityStateType[] StateTypes = Array.Empty<SerializableEntityStateType>();
            public bool AlwaysExecuteEnter;
            public float MinActiveDuration;
        }

        public static readonly SerializableEntityStateType SerializableStateType = new SerializableEntityStateType(typeof(MultiEntityState));

        EntityState[] _subStates = Array.Empty<EntityState>();
        Type[] _subStateTypes = Array.Empty<Type>();

        float _minActiveDuration;
        float _enterExecuteTime = -1f;

        bool _alwaysExecuteEnter;

        bool _isWaitingForForceExitDestruction;
        public bool IsWaitingForForceExitDestruction => _isWaitingForForceExitDestruction;

        public MultiEntityState()
        {
        }

        public void Initialize(SubStatesData data)
        {
            _subStates = data.StateTypes.Select(EntityStateCatalog.InstantiateState).ToArray();
            _subStateTypes = data.StateTypes.Select(state => state.stateType).ToArray();

            _alwaysExecuteEnter = data.AlwaysExecuteEnter;

            _minActiveDuration = data.MinActiveDuration;
        }

        public void OnOuterStateMachineAssigned()
        {
            foreach (EntityState state in _subStates)
            {
                state.outer = outer;

                if (state is MultiEntityState multiState)
                {
                    multiState.OnOuterStateMachineAssigned();
                }
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.WritePackedUInt32((uint)_subStates.Length);
            for (int i = 0; i < _subStates.Length; i++)
            {
                writer.Write(EntityStateCatalog.GetStateIndex(_subStateTypes[i]));
                _subStates[i].OnSerialize(writer);
            }

            writer.Write(_minActiveDuration);

            writer.WriteBits(_alwaysExecuteEnter, _isWaitingForForceExitDestruction);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            uint length = reader.ReadPackedUInt32();

            _subStates = new EntityState[length];
            _subStateTypes = new Type[length];
            for (int i = 0; i < length; i++)
            {
                EntityState state = _subStates[i] = EntityStateCatalog.InstantiateState(reader.ReadEntityStateIndex());
                _subStateTypes[i] = state.GetType();
                state.OnDeserialize(reader);
            }

            _minActiveDuration = reader.ReadSingle();

            reader.ReadBits(out _alwaysExecuteEnter, out _isWaitingForForceExitDestruction);
        }

        public override void OnEnter()
        {
            onEnter(null);
        }

        void onEnter(HashSet<Type> exclude)
        {
            for (int i = 0; i < _subStates.Length; i++)
            {
                EntityState state = _subStates[i];
                if (exclude == null || !exclude.Contains(_subStateTypes[i]))
                {
                    state.OnEnter();
                }
            }

            _enterExecuteTime = Time.time;
        }

        internal void forceExit(HashSet<Type> excludeExitTypes, HashSet<Type> excludeEnterTypes)
        {
            if (_subStates == null)
                return;

            if (_alwaysExecuteEnter && _enterExecuteTime < 0f)
            {
                Log.Debug("Attempting to exit state without OnEnter, executing enter now");
                onEnter(excludeEnterTypes);
            }

            for (int i = 0; i < _subStates.Length; i++)
            {
                EntityState state = _subStates[i];
                if (state != null && (excludeExitTypes == null || !excludeExitTypes.Contains(_subStateTypes[i])))
                {
                    try
                    {
                        state.OnExit();
                    }
                    catch (Exception e)
                    {
                        Log.Warning($"Caught exception when exiting {nameof(MultiEntityState)} due to state machine destroyed: {e}");
                    }
                }
            }

            IEnumerator routine = waitThenDestroyFromForceExit();
            if (outer)
            {
                outer.StartCoroutine(routine);
            }
            else
            {
                Main.Instance.StartCoroutine(routine);
            }
        }

        IEnumerator waitThenDestroyFromForceExit()
        {
            _isWaitingForForceExitDestruction = true;

            float timeSinceEnter = Time.time - _enterExecuteTime;
            if (timeSinceEnter < _minActiveDuration)
            {
                yield return new WaitForSeconds(_minActiveDuration - timeSinceEnter);
            }

            _isWaitingForForceExitDestruction = false;
            GameObject.Destroy(outer.gameObject);
            yield break;
        }

        public override void OnExit()
        {
            foreach (EntityState state in _subStates)
            {
                state.OnExit();
            }
        }

        public override void ModifyNextState(EntityState nextState)
        {
            foreach (EntityState state in _subStates)
            {
                state.ModifyNextState(nextState);
            }
        }

        public override void Update()
        {
            foreach (EntityState state in _subStates)
            {
                state.Update();
            }
        }

        public override void FixedUpdate()
        {
            foreach (EntityState state in _subStates)
            {
                state.FixedUpdate();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return (InterruptPriority)_subStates.Select(state => (int)state.GetMinimumInterruptPriority()).Max();
        }

        public override void PlayAnimation(string layerName, string animationStateName)
        {
            foreach (EntityState state in _subStates)
            {
                state.PlayAnimation(layerName, animationStateName);
            }
        }
    }
}
