using System;

namespace RoR2Randomizer.RandomizerController.Stage
{
    [Flags]
    public enum StageFlags : byte
    {
        None = 0,
        FirstStageBlacklist = 1 << 0
    }

    public readonly struct StageRandomizingInfo
    {
        public readonly string SceneName;
        public readonly StageFlags Flags;

        public StageRandomizingInfo(string sceneName, StageFlags flags)
        {
            SceneName = sceneName;
            Flags = flags;
        }
    }
}
