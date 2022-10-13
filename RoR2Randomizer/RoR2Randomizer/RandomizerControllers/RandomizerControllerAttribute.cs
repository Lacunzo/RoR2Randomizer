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
    public sealed class RandomizerControllerAttribute : HG.Reflection.SearchableAttribute
    {
    }
}
