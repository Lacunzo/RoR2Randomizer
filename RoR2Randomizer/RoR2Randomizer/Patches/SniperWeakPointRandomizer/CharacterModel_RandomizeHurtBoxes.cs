using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.SniperWeakPointRandomizer;
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
            if (NetworkServer.active && ConfigManager.Misc.SniperWeakPointRandomizerEnabled && self.TryGetComponent<HurtBoxGroup>(out HurtBoxGroup hurtBoxGroup))
            {
                HurtBox[] hurtBoxes = hurtBoxGroup.hurtBoxes;
                int numHurtBoxes = hurtBoxes.Length;
                if (numHurtBoxes > 1)
                {
                    bool[] originalIsSniperTargetValues = new bool[numHurtBoxes];

                    WeightedSelection<int> indexSelection = new WeightedSelection<int>(numHurtBoxes);

                    int totalSniperHurtBoxes = 0;
                    for (int i = 0; i < numHurtBoxes; i++)
                    {
                        HurtBox hurtBox = hurtBoxes[i];
                        if (hurtBox)
                        {
                            ref bool isSniperTarget = ref hurtBox.isSniperTarget;

                            originalIsSniperTargetValues[i] = isSniperTarget;

                            indexSelection.AddChoice(i, isSniperTarget ? 0.3f : 1f);

                            if (isSniperTarget)
                            {
                                totalSniperHurtBoxes++;
                                isSniperTarget = false;
                            }
                        }
                    }

                    while (totalSniperHurtBoxes > 0)
                    {
                        int choiceIndex = indexSelection.EvaluateToChoiceIndex(UnityEngine.Random.value);

                        indexSelection.RemoveChoice(choiceIndex);
                        HurtBox hurtBox = hurtBoxes[indexSelection.GetChoice(choiceIndex).value];

                        hurtBox.isSniperTarget = true;
                        totalSniperHurtBoxes--;
                    }

                    CharacterBody ownerBody = self.body;
                    if (ownerBody)
                    {
                        for (int i = 0; i < numHurtBoxes; i++)
                        {
                            HurtBox hurtBox = hurtBoxes[i];
                            if (hurtBox.isSniperTarget != originalIsSniperTargetValues[i])
                            {
#if DEBUG
                                Log.Debug($"Sending weak point override {hurtBox} {nameof(HurtBox.isSniperTarget)}={hurtBox.isSniperTarget}");
#endif
                                new SyncSniperWeakPointReplacement(ownerBody, hurtBox).SendTo(NetworkDestination.Clients);
                            }
                        }
                    }
                }
            }

            orig(self);
        }
    }
}
