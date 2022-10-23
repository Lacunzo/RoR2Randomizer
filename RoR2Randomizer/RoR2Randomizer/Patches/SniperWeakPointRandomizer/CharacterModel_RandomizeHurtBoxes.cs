using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.SniperWeakPointRandomizer;
using RoR2Randomizer.RandomizerControllers.SniperWeakPoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.SniperWeakPointRandomizer
{
    [PatchClass]
    static class CharacterModel_RandomizeHurtBoxes
    {
        static void Apply()
        {
            On.RoR2.CharacterModel.Awake += CharacterModel_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.CharacterModel.Awake -= CharacterModel_Awake;
        }

        static void CharacterModel_Awake(On.RoR2.CharacterModel.orig_Awake orig, CharacterModel self)
        {
            if (self.TryGetComponent<HurtBoxGroup>(out HurtBoxGroup hurtBoxGroup))
            {
                SniperWeakPointRandomizerController.TryRandomizeSniperTargets(hurtBoxGroup, self.body);
            }

            orig(self);
        }
    }
}
