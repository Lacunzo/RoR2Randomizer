using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public static class SpawnCardTracker
    {
        public static SpawnCard MithrixNormalSpawnCard;
        public static SpawnCard MithrixHurtSpawnCard;
        public static SpawnCard[] MithrixPhase2SpawnCards;

        public static SpawnCard[] VoidlingPhasesSpawnCards;

        public static SpawnCard AurelioniteSpawnCard;

        public static bool IsPartOfMithrixPhase2(SpawnCard card)
        {
            return MithrixPhase2SpawnCards != null && Array.IndexOf(MithrixPhase2SpawnCards, card) != -1;
        }

        public static bool IsAnyVoidlingPhase(SpawnCard card)
        {
            return VoidlingPhasesSpawnCards != null && Array.IndexOf(VoidlingPhasesSpawnCards, card) != -1;
        }
    }
}
