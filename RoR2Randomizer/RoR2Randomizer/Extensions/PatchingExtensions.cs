using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.Utility.Patching;
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
    }
}
