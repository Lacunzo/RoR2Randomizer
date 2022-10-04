using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.Utility;
using System;
using System.Linq;
using System.Reflection;

namespace RoR2Randomizer.Extensions
{
    public static class PatchingExtensions
    {
        public static bool MatchImplicitConversion<TFrom, TTo>(this Instruction instruction)
        {
            return MatchImplicitConversion(instruction, typeof(TFrom), typeof(TTo));
        }

        public static bool MatchImplicitConversion(this Instruction instruction, Type from, Type to)
        {
            return instruction.MatchCall(ReflectionUtils.FindImplicitConversion(from, to));
        }

        public static bool MatchGetMemberValue(this Instruction instruction, out MemberReference member)
        {
            if (instruction.MatchCallOrCallvirt(out MethodReference method))
            {
                member = method;
                return true;
            }

            FieldReference field;
            if (instruction.MatchLdfld(out field) ||
                instruction.MatchLdflda(out field) ||
                instruction.MatchLdsfld(out field) ||
                instruction.MatchLdsflda(out field))
            {
                member = field;
                return true;
            }

            member = null;
            return false;
        }
    }
}
