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
        public T Value { get; private set; }

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
            if (HasValue = _getNewValue(out T value))
                Value = value;
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
