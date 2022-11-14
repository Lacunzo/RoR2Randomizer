using EntityStates;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Utility;
using System;
using System.Linq;
using UnityEngine;

namespace RoR2Randomizer.ChildTransformAdditions
{
    public static class CustomChildTransformManager
    {
        [Flags]
        public enum ChildFlags : byte
        {
            None = 0,
            ForceNewObject = 1 << 0
        }

        public static void AutoAddChildTransform(EntityState state, string name, ChildFlags flags = ChildFlags.None)
        {
            if (state != null)
            {
                AutoAddChildTransform(state.characterBody, state.GetModelChildLocator(), name, flags);
            }
        }

        public static void AutoAddChildTransform(CharacterBody body, ChildLocator locator, string name, ChildFlags flags = ChildFlags.None)
        {
            if (!body || !locator || locator.FindChild(name))
                return;

            MiscUtils.AddItem(ref locator.transformPairs, getCustomChildTransformPair(body, locator, name, flags));

#if DEBUG
            Log.Debug($"Add child {name} to {body.GetDisplayName()}");
#endif
        }

        public static void AutoAddChildTransforms(EntityState state, params string[] names)
        {
            if (state != null)
            {
                AutoAddChildTransforms(state.characterBody, state.GetModelChildLocator(), names);
            }
        }

        public static void AutoAddChildTransforms(CharacterBody body, ChildLocator locator, params string[] names)
        {
            if (!body || !locator || names.Length == 0)
                return;

            MiscUtils.AddItems(ref locator.transformPairs, names.Where(n => !locator.FindChild(n)).Select(n =>
            {
                ChildLocator.NameTransformPair pair = getCustomChildTransformPair(body, locator, n, ChildFlags.None);

#if DEBUG
                Log.Debug($"Add child {n} to {body.GetDisplayName()}");
#endif

                return pair;
            }));
        }

        static ChildLocator.NameTransformPair getCustomChildTransformPair(CharacterBody body, ChildLocator locator, string name, ChildFlags flags)
        {
            Transform child = getCustomChildTransform(body, locator, name, flags);
            if ((flags & ChildFlags.ForceNewObject) != 0)
            {
                Transform transform = new GameObject(name).transform;

                transform.parent = child;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                child = transform;
            }

            return new ChildLocator.NameTransformPair
            {
                transform = child,
                name = name
            };
        }

        static Transform getCustomChildTransform(CharacterBody body, ChildLocator locator, string name, ChildFlags flags)
        {
            Transform result;
            switch (name)
            {
                case "MuzzleLeft" when locator.TryFindChild("HandL", out result):
                case "MuzzleRight" when locator.TryFindChild("HandR", out result):
                case "BodyCenter" when (bool)(result = body.coreTransform):
                    break;
                default:
                    if (!(result = body.aimOriginTransform))
                        result = body.transform;
                    break;
            }

            return result;
        }
    }
}
