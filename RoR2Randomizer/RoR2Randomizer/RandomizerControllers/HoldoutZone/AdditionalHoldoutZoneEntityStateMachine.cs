#if !DISABLE_HOLDOUT_ZONE_RANDOMIZER
using EntityStates;
using RoR2;
using RoR2Randomizer.Utility;
using System;

namespace RoR2Randomizer.RandomizerControllers.HoldoutZone
{
    public sealed class AdditionalHoldoutZoneEntityStateMachine : DependentEntityStateMachine
    {
        public RandomizedHoldoutZoneController Controller;

        public HoldoutZoneInfo OriginalZoneInfo;
        public HoldoutZoneInfo ReplacementZoneInfo;

        protected override bool onTargetStateMachineStateChanged(EntityState newState, out EntityState resultingState)
        {
            if (newState == null)
            {
                if (Controller)
                    Controller.State = HoldoutZoneStateType.Invalid;

                resultingState = null;
                return false;
            }

            Type newStateType = newState.GetType();

            Type resultType;
            if (newStateType == OriginalZoneInfo.StateCollection.Idle.stateType)
            {
                resultType = ReplacementZoneInfo.StateCollection.Idle.stateType;

                if (Controller)
                    Controller.State = HoldoutZoneStateType.Idle;
            }
            else if (newStateType == OriginalZoneInfo.StateCollection.IdleToCharging.stateType)
            {
                resultType = ReplacementZoneInfo.StateCollection.IdleToCharging.stateType;

                if (Controller)
                    Controller.State = HoldoutZoneStateType.IdleToCharging;
            }
            else if (newStateType == OriginalZoneInfo.StateCollection.Charging.stateType)
            {
                resultType = ReplacementZoneInfo.StateCollection.Charging.stateType;

                if (Controller)
                    Controller.State = HoldoutZoneStateType.Charging;
            }
            else if (newStateType == OriginalZoneInfo.StateCollection.Charged.stateType)
            {
                resultType = ReplacementZoneInfo.StateCollection.Charged.stateType;

                if (Controller)
                    Controller.State = HoldoutZoneStateType.Charged;
            }
            else if (newStateType == OriginalZoneInfo.StateCollection.Finished.stateType)
            {
                resultType = ReplacementZoneInfo.StateCollection.Finished.stateType;

                if (Controller)
                    Controller.State = HoldoutZoneStateType.Finished;
            }
            else
            {
                if (Controller)
                    Controller.State = HoldoutZoneStateType.Invalid;

                resultingState = null;
                return false;
            }

#if DEBUG
            Log.Debug($"{nameof(AdditionalHoldoutZoneEntityStateMachine)} setting state {newStateType.FullName} -> {resultType.FullName}");
#endif

            if (resultType != null)
            {
                if (ReplacementZoneInfo.SyncState)
                {
                    resultingState = EntityStateCatalog.InstantiateState(resultType);
                    return true;
                }
            }
            else
            {
                if (Controller)
                    Controller.State = HoldoutZoneStateType.Invalid;
            }

            resultingState = null;
            return false;
        }
    }
}
#endif