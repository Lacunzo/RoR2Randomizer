using RoR2;
using RoR2.Skills;
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

        public static bool TryFindChild(this ChildLocator locator, string name, out Transform child)
        {
            return child = locator.FindChild(name);
        }
    }
}
