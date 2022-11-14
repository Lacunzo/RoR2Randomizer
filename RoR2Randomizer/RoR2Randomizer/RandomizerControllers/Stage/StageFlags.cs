using System;

namespace RoR2Randomizer.RandomizerControllers.Stage
{
    [Flags]
    public enum StageFlags : byte
    {
        None = 0,
        FirstStageBlacklist = 1 << 0,
        PossibleStartingStage = 1 << 1,
        Inaccessible = 1 << 2,
        EndsRun = 1 << 3
    }
}
