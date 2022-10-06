using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Extensions
{
    public static class UnityObjectExtensions
    {
        public static Component GetOrAddComponent(this GameObject obj, Type componentType)
        {
            if (obj.TryGetComponent(componentType, out Component comp))
                return comp;

            return obj.AddComponent(componentType);
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            return (T)GetOrAddComponent(obj, typeof(T));
        }

        public static bool TryFindChild(this ChildLocator locator, string name, out Transform child)
        {
            return (bool)(child = locator.FindChild(name));
        }

        public static bool TryGetModelBounds(this CharacterModel root, out Bounds bounds)
        {
            if (root.mainSkinnedMeshRenderer)
            {
                bounds = root.mainSkinnedMeshRenderer.bounds;
                return true;
            }

            return root.transform.TryGetModelBounds(out bounds);
        }

        public static bool TryGetModelBounds(this Transform root, out Bounds bounds)
        {
            Bounds? result = null;

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (renderer && renderer.enabled && renderer is not ParticleSystemRenderer)
                {
                    if (result.HasValue)
                    {
                        Bounds tmp = result.Value;
                        tmp.Encapsulate(renderer.bounds);
                        result = tmp;
                    }
                    else
                    {
                        result = renderer.bounds;
                    }
                }
            }

            if (result.HasValue)
            {
                bounds = result.Value;
                return true;
            }
            else
            {
                bounds = default;
                return false;
            }
        }

        public static bool TryGetBodyRadius(this GameObject bodyPrefab, out float radius)
        {
            if (bodyPrefab.TryGetComponent<SphereCollider>(out SphereCollider sphereCollider))
            {
                radius = sphereCollider.radius;
                return true;
            }
            else if (bodyPrefab.TryGetComponent<CapsuleCollider>(out CapsuleCollider capsuleCollider))
            {
                radius = capsuleCollider.radius;
                return true;
            }
            else
            {
                radius = -1f;
                return false;
            }
        }
    }
}
