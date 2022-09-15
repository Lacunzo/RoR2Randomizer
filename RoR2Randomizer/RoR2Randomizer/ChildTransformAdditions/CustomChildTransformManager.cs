using EntityStates;
using RoR2;
using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Mono.Security.X509.X520;

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

            Array.Resize(ref locator.transformPairs, locator.transformPairs.Length + 1);
            locator.transformPairs[locator.transformPairs.Length - 1] = getCustomChildTransformPair(body, locator, name, flags);
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

            names = names.Where(n => !locator.FindChild(n)).ToArray();
            if (names.Length == 0)
                return;

            int prevLength = locator.transformPairs.Length;
            Array.Resize(ref locator.transformPairs, prevLength + names.Length);
            for (int i = prevLength; i < locator.transformPairs.Length; i++)
            {
                locator.transformPairs[i] = getCustomChildTransformPair(body, locator, names[i - prevLength], ChildFlags.None);
#if DEBUG
                Log.Debug($"Add child {names[i - prevLength]} to {body.GetDisplayName()}");
#endif
            }
        }

        static ChildLocator.NameTransformPair getCustomChildTransformPair(CharacterBody body, ChildLocator locator, string name, ChildFlags flags)
        {
            Transform child = getCustomChildTransform(body, locator, name, flags);
            if ((flags & ChildFlags.ForceNewObject) != 0)
            {
                GameObject newObj = new GameObject(name);

                Transform transform = newObj.transform;

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
                    return result;
                case "MuzzleRight" when locator.TryFindChild("HandR", out result):
                    return result;
                case "BodyCenter" when body.coreTransform:
                    return body.coreTransform;
            }

            if (body.aimOriginTransform)
                return body.aimOriginTransform;

            return body.transform;
        }
    }
}
