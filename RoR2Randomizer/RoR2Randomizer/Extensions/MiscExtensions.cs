using EntityStates;
using RoR2;
using RoR2.Skills;
using RoR2Randomizer.Networking.BossRandomizer;
using System;
using UnityEngine;

namespace RoR2Randomizer.Extensions
{
    public static class MiscExtensions
    {
        public static string ToLogString(this SkillFamily.Variant variant)
        {
            return $"[{Language.GetString(variant.skillDef.skillNameToken)} ({variant.skillDef.skillName}, {((ScriptableObject)variant.skillDef).name}), (acticationState.stateType: {variant.skillDef.activationState.stateType?.FullName ?? "null"})]";
        }

        public static bool IsValid(this BossReplacementType value)
        {
            return value > BossReplacementType.Invalid && value < BossReplacementType.Count;
        }

        public static bool IsNothing(this SerializableEntityStateType state)
        {
            Type stateType = state.stateType;
            return stateType == null || stateType == typeof(Uninitialized);
        }
    }
}
