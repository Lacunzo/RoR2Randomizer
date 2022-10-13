﻿using RoR2;
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
        public readonly SceneIndex SceneIndex;
        public readonly StageFlags Flags;

        public readonly float BaseSelectionWeight;

        public StageRandomizingInfo(SceneIndex sceneIndex, StageFlags flags, float baseSelectionWeight = 1f)
        {
            SceneIndex = sceneIndex;
            Flags = flags;
            BaseSelectionWeight = baseSelectionWeight;
        }
    }
}
