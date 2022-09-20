using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageRandomizer
{
    public static class ArtifactTrialFixPatch
    {
        public static void Apply()
        {
            On.RoR2.ArtifactTrialMissionController.Awake += ArtifactTrialMissionController_Awake;
        }

        public static void Cleanup()
        {
            On.RoR2.ArtifactTrialMissionController.Awake -= ArtifactTrialMissionController_Awake;
        }

        static void ArtifactTrialMissionController_Awake(On.RoR2.ArtifactTrialMissionController.orig_Awake orig, RoR2.ArtifactTrialMissionController self)
        {
            // Artifact trial active, but no artifact selected, assume stage randomizer brought us here
            if (ConfigManager.StageRandomizer.Enabled && NetworkServer.active && !ArtifactTrialMissionController.trialArtifact)
            {
                List<int> availableArtifactIndices = null;

                if (RunArtifactManager.instance)
                {
                    availableArtifactIndices = new List<int>();

                    bool[] enabledArtifactsBitArray = RunArtifactManager.instance._enabledArtifacts;
                    for (int i = 0; i < enabledArtifactsBitArray.Length; i++)
                    {
                        if (!enabledArtifactsBitArray[i]) // Add index to available if the artifact is not enabled
                        {
                            availableArtifactIndices.Add(i);
                        }
                    }
                }

                if (availableArtifactIndices == null || availableArtifactIndices.Count == 0) // If there are no unused artifacts, just use any artifact
                    availableArtifactIndices = Enumerable.Range(0, ArtifactCatalog.artifactCount).ToList();

                ArtifactTrialMissionController.trialArtifact = ArtifactCatalog.GetArtifactDef((ArtifactIndex)availableArtifactIndices.GetRandomOrDefault());
            }

            orig(self);
        }
    }
}
