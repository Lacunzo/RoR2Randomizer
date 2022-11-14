using RoR2Randomizer.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.Utility
{
    public class TempComponentsTracker : MonoBehaviour
    {
        readonly List<Component> _trackedComponents = new List<Component>();

        void track(Component component)
        {
            _trackedComponents.Add(component);
        }

        public void RemoveAll()
        {
            foreach (Component component in _trackedComponents)
            {
                if (component)
                {
                    Destroy(component);
                }
            }

            Destroy(this);
        }

        public bool TryRemove<T>() where T : Component
        {
            return TryRemove(typeof(T));
        }

        public bool TryRemove(Type componentType)
        {
            for (int i = _trackedComponents.Count - 1; i >= 0; i--)
            {
                if (_trackedComponents[i])
                {
                    if (_trackedComponents[i].GetType() == componentType)
                    {
                        Destroy(_trackedComponents[i]);
                        _trackedComponents.RemoveAt(i);

                        if (_trackedComponents.Count == 0)
                            Destroy(this);

                        return true;
                    }
                }
                else
                {
                    _trackedComponents.RemoveAt(i);
                }
            }

            if (_trackedComponents.Count == 0)
                Destroy(this);

            return false;
        }

        public static T AddTempComponent<T>(MonoBehaviour owner) where T : Component
        {
            return (T)AddTempComponent(owner, typeof(T));
        }

        public static Component AddTempComponent(MonoBehaviour owner, Type type)
        {
            GameObject gameObject = owner.gameObject;

            Component component = gameObject.AddComponent(type);
            gameObject.GetOrAddComponent<TempComponentsTracker>().track(component);
            return component;
        }

        public static TempComponentsTracker AddTempComponents(MonoBehaviour owner, params Type[] componentTypes)
        {
            GameObject gameObject = owner.gameObject;

            TempComponentsTracker tracker = gameObject.GetOrAddComponent<TempComponentsTracker>();

            foreach (Type componentType in componentTypes)
            {
                tracker.track(gameObject.AddComponent(componentType));
            }

            return tracker;
        }

        public static bool RemoveAllTempComponents(MonoBehaviour owner)
        {
            TempComponentsTracker tracker = owner.GetComponent<TempComponentsTracker>();
            if (!tracker)
                return false;

            tracker.RemoveAll();
            return true;
        }

        public static bool TryRemoveTempComponent<T>(MonoBehaviour owner) where T : Component
        {
            return TryRemoveTempComponent(owner, typeof(T));
        }

        public static bool TryRemoveTempComponent(MonoBehaviour owner, Type componentType)
        {
            if (!owner)
                return false;

            TempComponentsTracker tracker = owner.GetComponent<TempComponentsTracker>();
            return tracker && tracker.TryRemove(componentType);
        }
    }
}
