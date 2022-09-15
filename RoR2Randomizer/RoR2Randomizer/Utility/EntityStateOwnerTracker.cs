using EntityStates;
using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public static class EntityStateOwnerTracker
    {
        static readonly Dictionary<EntityState, GenericSkill> _stateToOwnerCache = new Dictionary<EntityState, GenericSkill>();

        public static GenericSkill SkillOwnerForNextCall;

        public static void AddSkillOwner(GenericSkill skill, EntityState state)
        {
            if (skill && state != null)
            {
                _stateToOwnerCache[state] = skill;
            }
        }

        public static GenericSkill GetOwner(EntityState state)
        {
            if (_stateToOwnerCache.TryGetValue(state, out GenericSkill owner))
                return owner;

            if (state is ISkillState skillState)
                return skillState.activatorSkillSlot;

            return null;
        }

        public static void SetNextOwnerToOwnerOfState(EntityState state)
        {
            GenericSkill owner = GetOwner(state);
            if (owner)
            {
                SkillOwnerForNextCall = owner;
            }
        }
    }
}
