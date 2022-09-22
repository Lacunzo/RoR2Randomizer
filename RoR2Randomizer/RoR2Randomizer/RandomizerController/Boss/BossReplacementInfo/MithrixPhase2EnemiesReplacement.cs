using RoR2;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Utility;
using System.Collections;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public sealed class MithrixPhase2EnemiesReplacement : BaseMithrixReplacement
    {
        protected override BossReplacementType ReplacementType => BossReplacementType.MithrixPhase2;

        protected override IEnumerator initializeClient()
        {
            yield return base.initializeClient();

            CoroutineOut<CharacterBody> bodyOut = new CoroutineOut<CharacterBody>();
            yield return getBody(bodyOut);

            CharacterBody body = bodyOut.Result;

#if DEBUG
            Log.Debug($"{nameof(MithrixPhase2EnemiesReplacement)} {nameof(initializeClient)}: _master={_master}, body={(bool)body}");
#endif

            if (body)
            {
#if DEBUG
                Log.Debug($"{nameof(MithrixPhase2EnemiesReplacement)} {nameof(initializeClient)}: body.subtitleNameToken={body.subtitleNameToken}");
#endif
                if (string.IsNullOrEmpty(body.subtitleNameToken))
                {
                    body.subtitleNameToken = RNGUtils.Choose("LUNARWISP_BODY_SUBTITLE", "LUNARGOLEM_BODY_SUBTITLE", "LUNAREXPLODER_BODY_SUBTITLE");
                }
            }
        }
    }
}
