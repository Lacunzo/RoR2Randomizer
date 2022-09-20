using RoR2;
using RoR2Randomizer.Patches.StageRandomizer;
using RoR2Randomizer.RandomizerController;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches
{
    public static class StageRandomizerPatcher
    {
        public static void Apply()
        {
            ArtifactTrialFixPatch.Apply();
            InitializeStageReplacementsPatch.Apply();
            OverrideSceneLoadPatch.Apply();
        }

        public static void Cleanup()
        {
            ArtifactTrialFixPatch.Cleanup();
            InitializeStageReplacementsPatch.Cleanup();
            OverrideSceneLoadPatch.Cleanup();
        }
    }
}
