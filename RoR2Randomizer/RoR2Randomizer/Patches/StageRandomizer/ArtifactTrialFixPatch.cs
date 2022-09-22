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
            if (NetworkServer.active && ConfigManager.StageRandomizer.Enabled && !ArtifactTrialMissionController.trialArtifact)
            {
                IEnumerable<int> availableArtifactIndices = Enumerable.Range(0, ArtifactCatalog.artifactCount);

                if (RunArtifactManager.instance)
                {
                    IEnumerable<int> onlyDisabledArtifactIndices = availableArtifactIndices.Where(i => !RunArtifactManager.instance._enabledArtifacts[i]);
                    if (onlyDisabledArtifactIndices.Any())
                        availableArtifactIndices = onlyDisabledArtifactIndices;
                }

                ArtifactTrialMissionController.trialArtifact = ArtifactCatalog.GetArtifactDef((ArtifactIndex)availableArtifactIndices.GetRandomOrDefault());
            }

            orig(self);
        }
    }
}
