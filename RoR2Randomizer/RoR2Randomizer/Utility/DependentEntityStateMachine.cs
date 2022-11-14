#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using RoR2;
using System.Collections.Generic;

namespace RoR2Randomizer.Utility
{
    public abstract class DependentEntityStateMachine : EntityStateMachine
    {
        static readonly HashSet<DependentEntityStateMachine> _instances = new HashSet<DependentEntityStateMachine>();

        bool _isInitialized;

        EntityStateMachine _target;

        public void Initialize(EntityStateMachine target)
        {
            if (_isInitialized)
                return;

            _instances.Add(this);

            _target = target;

            _isInitialized = true;
        }

        protected abstract bool onTargetStateMachineStateChanged(EntityState newState, out EntityState resultingState);

        public static void OnEntityStateAssigned(EntityStateMachine stateMachine, EntityState newState)
        {
            _instances.RemoveWhere(e => !e);
            foreach (DependentEntityStateMachine instance in _instances)
            {
                if (instance._target == stateMachine)
                {
                    if (instance.onTargetStateMachineStateChanged(newState, out EntityState resultingState))
                    {
                        instance.SetState(resultingState);
                    }
                    else
                    {
                        instance.SetState(new Idle());
                    }
                }
            }
        }
    }
}
#endif