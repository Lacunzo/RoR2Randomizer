using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public class RunSpecific<T> : IDisposable
    {
        public delegate bool TryGetNewValueDelegate(out T value);

        readonly ulong _callbackHandle;

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

        public RunSpecific(int priority = 0, T defaultValue = default) : this(getNewValue: null, priority, defaultValue)
        {
        }

        public RunSpecific(Func<T> getValueFunc, int priority = 0, T defaultValue = default) : this((out T value) => { value = getValueFunc(); return true; }, priority, defaultValue)
        {
        }

        public RunSpecific(TryGetNewValueDelegate getNewValue, int priority = 0, T defaultValue = default)
        {
            _getNewValue = getNewValue;
            Value = (_defaultValue = defaultValue);
            HasValue = false;

            _callbackHandle = RunSpecificCallbacksManager.AddEntry(onRunStart, onRunEnd, priority);
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
        }

        public void Dispose()
        {
            RunSpecificCallbacksManager.RemoveEntry(_callbackHandle);
        }

        public static implicit operator T(RunSpecific<T> runSpecific)
        {
            return runSpecific.Value;
        }
    }
}
