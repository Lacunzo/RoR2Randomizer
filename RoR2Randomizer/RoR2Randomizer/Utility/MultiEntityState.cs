using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Utility
{
    public sealed class MultiEntityState : EntityState
    {
        public static readonly SerializableEntityStateType SerializableStateType = new SerializableEntityStateType(typeof(MultiEntityState));

        EntityState[] _subStates = Array.Empty<EntityState>();

        public MultiEntityState()
        {
        }

        public void SetStates(SerializableEntityStateType[] stateTypes)
        {
            _subStates = stateTypes.Select(EntityStateCatalog.InstantiateState).ToArray();
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
            foreach (EntityState state in _subStates)
            {
                writer.Write(EntityStateCatalog.GetStateIndex(state.GetType()));
                state.OnSerialize(writer);
            }
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            uint length = reader.ReadPackedUInt32();
            
            _subStates = new EntityState[length];
            for (int i = 0; i < length; i++)
            {
                EntityState state = _subStates[i] = EntityStateCatalog.InstantiateState(reader.ReadEntityStateIndex());
                state.OnDeserialize(reader);
            }
        }

        public override void OnEnter()
        {
            foreach (EntityState state in _subStates)
            {
                state.OnEnter();
            }
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
