using R2API;
using RoR2;
using RoR2Randomizer.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    static class HereticNameInDialogueOverrideManager
    {
        static LanguageAPI.LanguageOverlay[] _hereticNameOverlays;

        [SystemInitializer]
        static void Init()
        {
            Run.onRunDestroyGlobal += onRunEnd;

            FullExplicitSpawnInitListener.OnFullInit += FullExplicitSpawnInitListener_OnFullInit;
        }

        static void FullExplicitSpawnInitListener_OnFullInit()
        {
            if (ExplicitSpawnRandomizerController.TryGetReplacementBodyIndex(Caches.Bodies.HereticBodyIndex, out BodyIndex replacementIndex))
            {
                GameObject replacementBodyPrefab = BodyCatalog.GetBodyPrefab(replacementIndex);
                if (replacementBodyPrefab && replacementBodyPrefab.TryGetComponent<CharacterBody>(out CharacterBody body))
                {
                    _hereticNameOverlays = replaceHereticNames(body.baseNameToken);
                }
            }
        }

        static LanguageAPI.LanguageOverlay[] replaceHereticNames(string replacementBodyNameToken)
        {
            const string LOG_PREFIX = $"{nameof(HereticNameInDialogueOverrideManager)}.{nameof(replaceHereticNames)} ";

            Language[] languages = Language.GetAllLanguages().ToArray();

            LanguageAPI.LanguageOverlay[] result = new LanguageAPI.LanguageOverlay[languages.Length];
            for (int i = 0; i < languages.Length; i++)
            {
                string languageName = languages[i].name;
                Dictionary<string, string> tokenDictionary = new Dictionary<string, string>();
                void replaceHereticNameReference(string baseToken, bool uppercase)
                {
                    string baseText = Language.GetString(baseToken, languageName);

                    string oldValue = Language.GetString("HERETIC_BODY_NAME", languageName);
                    string newValue = Language.GetString(replacementBodyNameToken, languageName);

                    if (uppercase)
                    {
                        oldValue = oldValue.ToUpper();
                        newValue = newValue.ToUpper();
                    }

                    tokenDictionary.Add(baseToken, baseText.Replace(oldValue, newValue));
                }

                replaceHereticNameReference("BROTHER_SEE_HERETIC_1", false);
                replaceHereticNameReference("BROTHER_SEE_HERETIC_2", false);

                replaceHereticNameReference("BROTHER_KILL_HERETIC_1", false);
                replaceHereticNameReference("BROTHER_KILL_HERETIC_2", false);

                replaceHereticNameReference("BROTHERHURT_SEE_TITANGOLD_AND_HERETIC_1", true);

                replaceHereticNameReference("BROTHERHURT_KILL_HERETIC_1", true);
                replaceHereticNameReference("BROTHERHURT_KILL_HERETIC_2", true);

                result[i] = LanguageAPI.AddOverlay(tokenDictionary, languageName);
            }

            return result;
        }

        static void onRunEnd(Run _)
        {
            if (_hereticNameOverlays != null)
            {
                foreach (LanguageAPI.LanguageOverlay overlay in _hereticNameOverlays)
                {
                    overlay?.Remove();
                }

                _hereticNameOverlays = null;
            }
        }
    }
}
