using RoR2;
using RoR2Randomizer.RandomizerControllers.SurvivorPod;
using UnityEngine;

namespace RoR2Randomizer.Patches.SurvivorPodRandomizer
{
    [PatchClass]
    public static class OverrideIntroAnimation
    {
        static void Apply()
        {
            On.RoR2.Run.HandlePlayerFirstEntryAnimation += Run_HandlePlayerFirstEntryAnimation;
            On.RoR2.InfiniteTowerRun.HandlePlayerFirstEntryAnimation += InfiniteTowerRun_HandlePlayerFirstEntryAnimation;
        }

        static void Cleanup()
        {
            On.RoR2.Run.HandlePlayerFirstEntryAnimation -= Run_HandlePlayerFirstEntryAnimation;
            On.RoR2.InfiniteTowerRun.HandlePlayerFirstEntryAnimation -= InfiniteTowerRun_HandlePlayerFirstEntryAnimation;
        }

        static void Run_HandlePlayerFirstEntryAnimation(On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig, Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            SurvivorPodRandomizerController.TryOverrideIntroAnimation(body);

            orig(self, body, spawnPosition, spawnRotation);
        }

        static void InfiniteTowerRun_HandlePlayerFirstEntryAnimation(On.RoR2.InfiniteTowerRun.orig_HandlePlayerFirstEntryAnimation orig, InfiniteTowerRun self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            SurvivorPodRandomizerController.TryOverrideIntroAnimation(body);

            orig(self, body, spawnPosition, spawnRotation);
        }
    }
}
