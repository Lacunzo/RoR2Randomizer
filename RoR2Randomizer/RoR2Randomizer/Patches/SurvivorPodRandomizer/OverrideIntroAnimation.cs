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
            CharacterBody.onBodyAwakeGlobal += CharacterBody_onBodyAwakeGlobal;
        }

        static void Cleanup()
        {
            CharacterBody.onBodyAwakeGlobal -= CharacterBody_onBodyAwakeGlobal;
        }

        static void CharacterBody_onBodyAwakeGlobal(CharacterBody body)
        {
            SurvivorPodRandomizerController.TryOverrideIntroAnimation(body);
        }
    }
}
