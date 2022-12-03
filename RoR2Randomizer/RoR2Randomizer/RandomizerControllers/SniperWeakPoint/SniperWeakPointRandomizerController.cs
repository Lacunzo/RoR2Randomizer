using RoR2;
using RoR2Randomizer.Configuration;
using UnityEngine.Networking;

namespace RoR2Randomizer.RandomizerControllers.SniperWeakPoint
{
    public static class SniperWeakPointRandomizerController
    {
        public static bool IsEnabled => NetworkServer.active && ConfigManager.Misc.SniperWeakPointRandomizerEnabled;

        public static void TryRandomizeSniperTargets(HurtBoxGroup hurtBoxGroup)
        {
            if (!IsEnabled)
                return;

            HurtBox[] hurtBoxes = hurtBoxGroup.hurtBoxes;
            int numHurtBoxes = hurtBoxes.Length;
            if (numHurtBoxes <= 1)
                return;
            
            bool[] originalIsSniperTargetValues = new bool[numHurtBoxes];

            WeightedSelection<int> indexSelection = new WeightedSelection<int>(numHurtBoxes);

            bool?[] overrideIsSniperTargetValues = new bool?[numHurtBoxes];

            int totalSniperHurtBoxes = 0;
            for (int i = 0; i < numHurtBoxes; i++)
            {
                HurtBox hurtBox = hurtBoxes[i];
                if (hurtBox)
                {
                    bool isSniperTarget = hurtBox.isSniperTarget;

                    originalIsSniperTargetValues[i] = isSniperTarget;

                    indexSelection.AddChoice(i, isSniperTarget ? 0.3f : 1f);

                    if (isSniperTarget)
                    {
                        totalSniperHurtBoxes++;
                        overrideIsSniperTargetValues[i] = false;
                    }
                }
            }

            if (totalSniperHurtBoxes <= 0)
                return;

            while (totalSniperHurtBoxes > 0)
            {
                int choiceIndex = indexSelection.EvaluateToChoiceIndex(UnityEngine.Random.value);

                int hurtBoxIndex = indexSelection.GetChoice(choiceIndex).value;
                indexSelection.RemoveChoice(choiceIndex);

                if (originalIsSniperTargetValues[hurtBoxIndex])
                {
                    overrideIsSniperTargetValues[hurtBoxIndex] = null;
                }
                else
                {
                    overrideIsSniperTargetValues[hurtBoxIndex] = true;
                }

                totalSniperHurtBoxes--;
            }

            HurtBoxGroupRandomizerData hurtBoxGroupRandomizerData = hurtBoxGroup.gameObject.AddComponent<HurtBoxGroupRandomizerData>();
            hurtBoxGroupRandomizerData.Initialize(originalIsSniperTargetValues, overrideIsSniperTargetValues);
        }
    }
}
