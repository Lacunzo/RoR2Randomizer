using MonoMod.Cil;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerControllers;
using RoR2Randomizer.RandomizerControllers.Stage;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageRandomizer
{
    [PatchClass]
    public static class ArtifactTrialFixPatch
    {
        static void Apply()
        {
            On.RoR2.ArtifactTrialMissionController.Awake += ArtifactTrialMissionController_Awake;
            IL.RoR2.PortalDialerController.OpenArtifactPortalServer += PortalDialerController_OpenArtifactPortalServer;
        }

        static void Cleanup()
        {
            On.RoR2.ArtifactTrialMissionController.Awake -= ArtifactTrialMissionController_Awake;
            IL.RoR2.PortalDialerController.OpenArtifactPortalServer -= PortalDialerController_OpenArtifactPortalServer;
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

        static void PortalDialerController_OpenArtifactPortalServer(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(x => x.MatchStfld<ArtifactTrialMissionController>(nameof(ArtifactTrialMissionController.trialArtifact))))
            {
                c.EmitDelegate((ArtifactDef artifactDef) =>
                {
                    // If the artifact trial stage is replaced by something else, don't set the trial artifact definition
                    if (NetworkServer.active && ConfigManager.StageRandomizer.Enabled)
                    {
                        SceneIndex artifactTrialSceneIndex = StageRandomizerController.ArtifactTrialSceneIndex;
                        if (artifactTrialSceneIndex != SceneIndex.Invalid && StageRandomizerController.TryGetReplacementSceneIndex(artifactTrialSceneIndex, out SceneIndex replacement))
                        {
                            if (replacement != artifactTrialSceneIndex)
                            {
                                return null;
                            }
                        }
                    }

                    return artifactDef;
                });
            }
        }
    }
}
