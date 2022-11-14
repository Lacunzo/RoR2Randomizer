using EntityStates.Missions.LunarScavengerEncounter;
using RoR2;
using RoR2Randomizer.RandomizerControllers.Boss;
using RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.BossRandomizer.LunarScav
{
    [PatchClass]
    public sealed class LunarScavFightTracker : BossTracker<LunarScavFightTracker>
    {
        static void ApplyPatches()
        {
            new LunarScavFightTracker().applyPatches();
        }

        static void CleanupPatches()
        {
            Instance?.cleanupPatches();
        }

        protected override void applyPatches()
        {
            base.applyPatches();

            BossRandomizerController.LunarScav.LunarScavReplacementReceivedClient += LunarScavReplacementReceivedClient;

            On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.OnEnter += FadeOut_OnEnter;
        }

        protected override void cleanupPatches()
        {
            base.cleanupPatches();

            BossRandomizerController.LunarScav.LunarScavReplacementReceivedClient -= LunarScavReplacementReceivedClient;

            On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.OnEnter -= FadeOut_OnEnter;
        }

        void FadeOut_OnEnter(On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.orig_OnEnter orig, FadeOut self)
        {
            IsInFight.Value = false;

            orig(self);
        }

        void LunarScavReplacementReceivedClient(LunarScavReplacement replacement)
        {
            IsInFight.Value = true;
        }

        protected override void onSceneLoaded(SceneDef scene)
        {
            base.onSceneLoaded(scene);

            if (Caches.Scene.LunarScavFightSceneIndex != SceneIndex.Invalid && scene.sceneDefIndex == Caches.Scene.LunarScavFightSceneIndex)
            {
                IsInFight.Value = true;
            }
        }
    }
}
