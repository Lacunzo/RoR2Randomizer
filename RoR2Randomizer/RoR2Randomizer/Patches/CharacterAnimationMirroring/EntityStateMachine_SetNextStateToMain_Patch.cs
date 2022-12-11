using RoR2;
using RoR2Randomizer.RandomizerControllers.SurvivorPod;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Patches.CharacterAnimationMirroring
{
    [PatchClass]
    static class EntityStateMachine_SetNextStateToMain_Patch
    {
        static void Apply()
        {
            On.RoR2.EntityStateMachine.SetNextStateToMain += EntityStateMachine_SetNextStateToMain;
        }

        static void Cleanup()
        {
            On.RoR2.EntityStateMachine.SetNextStateToMain -= EntityStateMachine_SetNextStateToMain;
        }

        static void EntityStateMachine_SetNextStateToMain(On.RoR2.EntityStateMachine.orig_SetNextStateToMain orig, EntityStateMachine self)
        {
            orig(self);

            if (self.TryGetComponent<RandomizedIntroAnimationTracker>(out RandomizedIntroAnimationTracker introAnimationTracker))
            {
                introAnimationTracker.OnNextStateSetToMain();
            }
        }
    }
}
