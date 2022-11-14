#if !DISABLE_SKILL_RANDOMIZER
namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.AncientWispMonster
{
    [PatchClass]
    public static class ChannelRain
    {
        static void Apply()
        {
            IL.EntityStates.AncientWispMonster.ChannelRain.PlaceRain += Shared.Physics_Raycast_LayerMaskDistanceFix_ILPatch;
        }

        static void Cleanup()
        {
            IL.EntityStates.AncientWispMonster.ChannelRain.PlaceRain -= Shared.Physics_Raycast_LayerMaskDistanceFix_ILPatch;
        }
    }
}
#endif