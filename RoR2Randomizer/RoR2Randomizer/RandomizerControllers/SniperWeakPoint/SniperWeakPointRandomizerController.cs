using R2API.Networking;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Networking.SniperWeakPointRandomizer;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.SniperWeakPoint
{
    public static class SniperWeakPointRandomizerController
    {
        public static bool IsEnabled => NetworkServer.active && ConfigManager.Misc.SniperWeakPointRandomizerEnabled;

        public static void TryRandomizeSniperTargets(HurtBoxGroup hurtBoxGroup, CharacterBody ownerBody)
        {
            if (!IsEnabled)
                return;

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

                HurtBoxGroupRandomizerData hurtBoxGroupRandomizerData = hurtBoxGroup.gameObject.AddComponent<HurtBoxGroupRandomizerData>();
                hurtBoxGroupRandomizerData.OwnerBody = ownerBody;
                hurtBoxGroupRandomizerData.OriginalIsSniperTargetValues = originalIsSniperTargetValues;
            }
        }
    }
}
