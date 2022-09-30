using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public class RunSpecific<T> : IDisposable
    {
        public delegate bool TryGetNewValueDelegate(out T value);

        readonly TryGetNewValueDelegate _getNewValue;
        readonly T _defaultValue;

        public bool HasValue { get; private set; }

        T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                HasValue = !EqualityComparer<T>.Default.Equals(value, _defaultValue);
            }
        }

        public RunSpecific(T defaultValue = default) : this(null, defaultValue)
        {
        }

        public RunSpecific(TryGetNewValueDelegate getNewValue, T defaultValue = default)
        {
            _getNewValue = getNewValue;
            Value = (_defaultValue = defaultValue);
            HasValue = false;

            Run.onRunStartGlobal += onRunStart;
            Run.onRunDestroyGlobal += onRunEnd;
        }

        ~RunSpecific()
        {
            Dispose();
        }

        void onRunStart(Run instance)
        {
            if (_getNewValue != null && !HasValue)
            {
                if (_getNewValue(out T value))
                {
                    Value = value;
                }
                else
                {
                    Value = _defaultValue;
                }
            }
        }

        void onRunEnd(Run instance)
        {
            Value = _defaultValue;
            HasValue = false;
        }

        public void Dispose()
        {
            Run.onRunStartGlobal -= onRunStart;
            Run.onRunDestroyGlobal -= onRunEnd;
        }

        public static implicit operator T(RunSpecific<T> runSpecific)
        {
            return runSpecific.Value;
        }
    }
}
