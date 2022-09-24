#if !DISABLE_SKILL_RANDOMIZER
using EntityStates;
using RoR2;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.Fixes.EntityStateOwnerSkill
{
    public static class MainPatcher
    {
        public static void Apply()
        {
            On.RoR2.EntityStateCatalog.InstantiateState_Type += EntityStateCatalog_InstantiateState_Type;
            On.RoR2.EntityStateCatalog.InstantiateState_EntityStateIndex += EntityStateCatalog_InstantiateState_EntityStateIndex;

            On.RoR2.EntityStateMachine.SetNextState += EntityStateMachine_SetNextState;

            NextSkillOwnerSetters.RoR2.Skills.ComboSkillDef.Apply();
            NextSkillOwnerSetters.RoR2.Skills.ConditionalSkillDef.Apply();
            NextSkillOwnerSetters.RoR2.Skills.ReloadSkillDef.Apply();
            NextSkillOwnerSetters.RoR2.Skills.SkillDef.Apply();
        }

        public static void Cleanup()
        {
            On.RoR2.EntityStateCatalog.InstantiateState_Type -= EntityStateCatalog_InstantiateState_Type;
            On.RoR2.EntityStateCatalog.InstantiateState_EntityStateIndex -= EntityStateCatalog_InstantiateState_EntityStateIndex;

            On.RoR2.EntityStateMachine.SetNextState -= EntityStateMachine_SetNextState;

            NextSkillOwnerSetters.RoR2.Skills.ComboSkillDef.Cleanup();
            NextSkillOwnerSetters.RoR2.Skills.ConditionalSkillDef.Cleanup();
            NextSkillOwnerSetters.RoR2.Skills.ReloadSkillDef.Cleanup();
            NextSkillOwnerSetters.RoR2.Skills.SkillDef.Cleanup();
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