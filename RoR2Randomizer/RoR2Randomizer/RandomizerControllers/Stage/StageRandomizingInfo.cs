using RoR2;

namespace RoR2Randomizer.RandomizerControllers.Stage
{
    public readonly struct StageRandomizingInfo
    {
        public readonly SceneIndex SceneIndex;
        public readonly StageFlags Flags;

        public readonly float BaseSelectionWeight;

        public StageRandomizingInfo(SceneIndex sceneIndex, StageFlags flags, float baseSelectionWeight = 1f)
        {
            SceneIndex = sceneIndex;
            Flags = flags;
            BaseSelectionWeight = baseSelectionWeight;
        }

        public override string ToString()
        {
            return $"{SceneCatalog.GetSceneDef(SceneIndex).cachedName} {nameof(Flags)}={Flags:F}";
        }
    }
}
