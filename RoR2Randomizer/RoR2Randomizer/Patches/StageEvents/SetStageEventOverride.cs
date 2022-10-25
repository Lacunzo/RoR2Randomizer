using RoR2;
using RoR2Randomizer.Configuration;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.StageEvents
{
    [PatchClass]
    static class SetStageEventOverride
    {
        static FamilyDirectorCardCategorySelection _gupFamilySelection;

        static SetStageEventOverride()
        {
            _gupFamilySelection = ScriptableObject.CreateInstance<FamilyDirectorCardCategorySelection>();
            _gupFamilySelection.categories = new DirectorCardCategorySelection.Category[]
            {
                new DirectorCardCategorySelection.Category
                {
                    name = "Gup",
                    cards = new DirectorCard[]
                    {
                        new DirectorCard
                        {
                            spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscGupBody"),
                            selectionWeight = 1
                        }
                    },
                    selectionWeight = 1f
                }
            };

            _gupFamilySelection.selectionChatString = "FAMILY_GUP";
        }

        static void Apply()
        {
            On.RoR2.ClassicStageInfo.Awake += ClassicStageInfo_Awake;
        }

        static void Cleanup()
        {
            On.RoR2.ClassicStageInfo.Awake -= ClassicStageInfo_Awake;
        }

        static void ClassicStageInfo_Awake(On.RoR2.ClassicStageInfo.orig_Awake orig, ClassicStageInfo self)
        {
            OverrideStageEventPatch.ForcedCategorySelection = null;

            if (NetworkServer.active)
            {
                if (ConfigManager.Fun.GupModeActive)
                {
                    OverrideStageEventPatch.ForcedCategorySelection = _gupFamilySelection;
                }
            }

            orig(self);
        }
    }
}
