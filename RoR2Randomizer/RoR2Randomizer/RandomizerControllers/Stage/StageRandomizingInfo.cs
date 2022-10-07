using System;

namespace RoR2Randomizer.RandomizerControllers.Stage
{
    [Flags]
    public enum StageFlags : byte
    {
        None = 0,
        FirstStageBlacklist = 1 << 0,
        PossibleStartingStage = 1 << 1
    }

    public readonly struct StageRandomizingInfo
    {
        public readonly string SceneName;
        public readonly StageFlags Flags;

        public readonly float BaseSelectionWeight;

        public StageRandomizingInfo(string sceneName, StageFlags flags, float baseSelectionWeight = 1f)
        {
            SceneName = sceneName;
            Flags = flags;
            BaseSelectionWeight = baseSelectionWeight;
        }
    }
}
