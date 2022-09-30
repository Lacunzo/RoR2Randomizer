#if !DISABLE_SKILL_RANDOMIZER
using EntityStates;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill
{
    [PatchClass]
    public static class MainPatcher
    {
        static void Apply()
        {
            On.RoR2.EntityStateCatalog.InstantiateState_Type += EntityStateCatalog_InstantiateState_Type;
            On.RoR2.EntityStateCatalog.InstantiateState_EntityStateIndex += EntityStateCatalog_InstantiateState_EntityStateIndex;

            On.RoR2.EntityStateMachine.SetNextState += EntityStateMachine_SetNextState;
        }

        static void Cleanup()
        {
            On.RoR2.EntityStateCatalog.InstantiateState_Type -= EntityStateCatalog_InstantiateState_Type;
            On.RoR2.EntityStateCatalog.InstantiateState_EntityStateIndex -= EntityStateCatalog_InstantiateState_EntityStateIndex;

            On.RoR2.EntityStateMachine.SetNextState -= EntityStateMachine_SetNextState;
        }

        static void EntityStateMachine_SetNextState(On.RoR2.EntityStateMachine.orig_SetNextState orig, EntityStateMachine self, EntityState newNextState)
        {
            GenericSkill owner = EntityStateOwnerTracker.GetOwner(self.state);

            orig(self, newNextState);

            if (owner && !EntityStateOwnerTracker.GetOwner(self.nextState))
            {
                EntityStateOwnerTracker.AddSkillOwner(owner, self.nextState);
            }
        }

        static EntityState EntityStateCatalog_InstantiateState(Func<EntityState> invokeOrig)
        {
            GenericSkill skillOwner = EntityStateOwnerTracker.SkillOwnerForNextCall;
            EntityState result = invokeOrig();

            if (skillOwner && result != null)
            {
                EntityStateOwnerTracker.AddSkillOwner(skillOwner, result);
                EntityStateOwnerTracker.SkillOwnerForNextCall = null;
            }

            return result;
        }

        static EntityState EntityStateCatalog_InstantiateState_Type(On.RoR2.EntityStateCatalog.orig_InstantiateState_Type orig, Type stateType)
        {
            return EntityStateCatalog_InstantiateState(() => orig(stateType));
        }

        static EntityState EntityStateCatalog_InstantiateState_EntityStateIndex(On.RoR2.EntityStateCatalog.orig_InstantiateState_EntityStateIndex orig, EntityStateIndex entityStateIndex)
        {
            return EntityStateCatalog_InstantiateState(() => orig(entityStateIndex));
        }
    }
}
#endif