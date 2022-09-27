using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public static class ReflectionUtils
    {
        public static MethodInfo FindImplicitConversion(Type from, Type to)
        {
            const BindingFlags METHOD_FLAGS = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (MethodInfo method in from.GetMethods(METHOD_FLAGS).Concat(to.GetMethods(METHOD_FLAGS)))
            {
                if (method.IsSpecialName && method.Name == "op_Implicit" && method.ReturnType == to)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == from)
                    {
                        return method;
                    }
                }
            }

            throw new MissingMethodException($"No implicit conversion {from.FullDescription()} -> {to.FullDescription()} could be found");
        }

        public static LinkedList<Type> GetTypeHierarchyList(Type type)
        {
            return MiscUtils.CreateReverseLinkedListFromLinks(type, t => t.BaseType);
        }
    }
}
