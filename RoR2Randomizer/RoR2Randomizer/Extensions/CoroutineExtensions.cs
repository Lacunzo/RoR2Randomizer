using System.Collections;
using RoR2Randomizer.Utility;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.Extensions
{
    public static class CoroutineExtensions
    {
        public static IEnumerator AddTimeout(this IEnumerator baseRoutine, float timeout, CoroutineOut<TimeoutActionResult> result = null)
        {
            float timeStarted = Time.unscaledTime;

            while (baseRoutine.MoveNext())
            {
                float elapsed = Time.unscaledTime - timeStarted;
                if (elapsed >= timeout)
                {
#if DEBUG
                    Log.Debug($"Routine {baseRoutine} timed out");
#endif

                    if (result != null)
                        result.Result = new TimeoutActionResult(elapsed, TimeoutActionResultState.TimedOut);

                    yield break;
                }

                yield return baseRoutine.Current;
            }

            if (result != null)
                result.Result = new TimeoutActionResult(Time.unscaledTime - timeStarted, TimeoutActionResultState.Finished);
        }
    }
}
