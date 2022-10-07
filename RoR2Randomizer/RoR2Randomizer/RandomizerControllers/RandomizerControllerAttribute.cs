using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerControllers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RandomizerControllerAttribute : Attribute
    {
        public static readonly InitializeOnAccess<Type[]> RandomizerControllerTypes = new InitializeOnAccess<Type[]>(() =>
        {
            return (from type in Assembly.GetExecutingAssembly().GetTypes()
                    where type.GetCustomAttribute(typeof(RandomizerControllerAttribute)) != null
                    select type).ToArray();
        });
    }
}
