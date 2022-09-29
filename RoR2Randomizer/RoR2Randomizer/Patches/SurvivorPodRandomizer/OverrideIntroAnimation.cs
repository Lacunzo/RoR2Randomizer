using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.RandomizerController.SurvivorPod;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Patches.SurvivorPodRandomizer
{
    public static class OverrideIntroAnimation
    {
        public static void Apply()
        {
            On.RoR2.Run.HandlePlayerFirstEntryAnimation += Run_HandlePlayerFirstEntryAnimation;
            On.RoR2.InfiniteTowerRun.HandlePlayerFirstEntryAnimation += InfiniteTowerRun_HandlePlayerFirstEntryAnimation;
        }

        public static void Cleanup()
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
