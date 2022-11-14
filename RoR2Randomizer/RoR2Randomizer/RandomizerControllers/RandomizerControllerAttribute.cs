using System;

namespace RoR2Randomizer.RandomizerControllers
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RandomizerControllerAttribute : HG.Reflection.SearchableAttribute
    {
    }
}
