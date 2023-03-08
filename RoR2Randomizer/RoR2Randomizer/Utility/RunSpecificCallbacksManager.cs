using RoR2;
using System;
using System.Collections.Generic;

namespace RoR2Randomizer.Utility
{
    static class RunSpecificCallbacksManager
    {
        readonly struct CallbackEntry
        {
            static ulong _nextHandleIndex = 0;
            public readonly ulong Handle;

            public readonly Action<Run> OnStartRun;
            public readonly Action<Run> OnEndRun;

            readonly int _priority;

            public CallbackEntry(int priority, Action<Run> onStartRun, Action<Run> onEndRun)
            {
                _priority = priority;
                OnStartRun = onStartRun;
                OnEndRun = onEndRun;

                Handle = _nextHandleIndex;

                try
                {
                    checked
                    {
                        _nextHandleIndex++;
                    }
                }
                catch (OverflowException)
                {
                    _nextHandleIndex = 0;
                }
            }

            public static readonly IComparer<CallbackEntry> PriorityComparer = new Comparer();

            class Comparer : IComparer<CallbackEntry>
            {
                int IComparer<CallbackEntry>.Compare(CallbackEntry x, CallbackEntry y)
                {
                    // Sorts in descending order
                    return y._priority.CompareTo(x._priority);
                }
            }
        }

        static readonly List<CallbackEntry> _callbackEntries = new List<CallbackEntry>();

        static RunSpecificCallbacksManager()
        {
            Run.onRunStartGlobal += onRunStart;
            Run.onRunDestroyGlobal += onRunEnd;
        }

        static void onRunStart(Run instance)
        {
            foreach (CallbackEntry entry in _callbackEntries)
            {
                entry.OnStartRun?.Invoke(instance);
            }
        }

        static void onRunEnd(Run instance)
        {
            foreach (CallbackEntry entry in _callbackEntries)
            {
                entry.OnEndRun?.Invoke(instance);
            }
        }

        public static ulong AddEntry(Action onStart, Action onEnd, int priority)
        {
            static Action<Run> convertToRunCallback(Action original)
            {
                return original != null ? _ => original() : null;
            }

            return AddEntry(convertToRunCallback(onStart), convertToRunCallback(onEnd), priority);
        }

        public static ulong AddEntry(Action<Run> onStart, Action<Run> onEnd, int priority)
        {
            CallbackEntry entry = new CallbackEntry(priority, onStart, onEnd);
            _callbackEntries.Add(entry);
            _callbackEntries.Sort(CallbackEntry.PriorityComparer);
            return entry.Handle;
        }

        public static void RemoveEntry(ulong callbackHandle)
        {
            int index = _callbackEntries.FindIndex(c => c.Handle == callbackHandle);
            if (index != -1)
            {
                _callbackEntries.RemoveAt(index);
                // Removing an item cannot make the order incorrect (assuming it wasn't already), so there is no need to re-sort here
            }
            else
            {
                Log.Warning($"Attempted to remove entry with handle {callbackHandle}, but it was not found in the list");
            }
        }
    }
}
