using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityModdingUtility;

namespace RoR2Randomizer.Patches
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    sealed class PatchClassAttribute : Attribute
    {
        readonly struct PatchClassInfo
        {
            public readonly MethodInfo ApplyMethod;
            public readonly MethodInfo CleanupMethod;

            public PatchClassInfo(Type type)
            {
                const string APPLY_METHOD_NAME = "Apply";
                const string ALT_APPLY_METHOD_NAME = "ApplyPatches";

                const string CLEANUP_METHOD_NAME = "Cleanup";
                const string ALT_CLEANUP_METHOD_NAME = "CleanupPatches";

                const BindingFlags PATCH_METHOD_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

                ApplyMethod = type.GetMethod(APPLY_METHOD_NAME, PATCH_METHOD_FLAGS) ?? type.GetMethod(ALT_APPLY_METHOD_NAME, PATCH_METHOD_FLAGS);
                CleanupMethod = type.GetMethod(CLEANUP_METHOD_NAME, PATCH_METHOD_FLAGS) ?? type.GetMethod(ALT_CLEANUP_METHOD_NAME, PATCH_METHOD_FLAGS);

                if (ApplyMethod == null)
                    throw new MissingMethodException($"Patch class {type.FullName} does not have a {APPLY_METHOD_NAME} or {ALT_APPLY_METHOD_NAME} method");

                if (CleanupMethod == null)
                    throw new MissingMethodException($"Patch class {type.FullName} does not have a {CLEANUP_METHOD_NAME} or {ALT_CLEANUP_METHOD_NAME} method");
            }

            public void Apply()
            {
                ApplyMethod.Invoke(null, Array.Empty<object>());

#if DEBUG
                Log.Debug($"Applied patch class '{ApplyMethod.DeclaringType.FullName}'");
#endif
            }

            public void Cleanup()
            {
                CleanupMethod.Invoke(null, Array.Empty<object>());

#if DEBUG
                Log.Debug($"Cleaned up patch class '{CleanupMethod.DeclaringType.FullName}'");
#endif
            }
        }

        static readonly InitializeOnAccess<PatchClassInfo[]> _patchClasses = new InitializeOnAccess<PatchClassInfo[]>(() =>
        {
            return (from type in Assembly.GetExecutingAssembly().GetTypes()
                    where type.GetCustomAttribute(typeof(PatchClassAttribute)) != null
                    select new PatchClassInfo(type)).ToArray();
        });

        public static void ApplyAllPatches()
        {
            foreach (PatchClassInfo patchClass in _patchClasses.Get)
            {
                patchClass.Apply();
            }
        }

        public static void CleanupAllPatches()
        {
            foreach (PatchClassInfo patchClass in _patchClasses.Get)
            {
                patchClass.Cleanup();
            }
        }
    }
}
