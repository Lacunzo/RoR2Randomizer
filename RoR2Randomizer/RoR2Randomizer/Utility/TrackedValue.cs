using System;
using System.Collections.Generic;

namespace RoR2Randomizer.Utility
{
    public struct TrackedValue<T>
    {
        readonly IEqualityComparer<T> _comparer;
        T _value;

        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (MiscUtils.TryAssign(ref _value, value, _comparer))
                {
                    OnChanged?.Invoke(value);
                }
            }
        }

        public TrackedValue(T value = default, IEqualityComparer<T> comparer = null)
        {
            _value = value;
            _comparer = comparer;
        }

        public event Action<T> OnChanged;

        public static implicit operator T(TrackedValue<T> tracked)
        {
            return tracked.Value;
        }
    }
}
