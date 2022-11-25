using R2API;
using RoR2;
using RoR2Randomizer.Configuration.ConfigValue;
using System.Collections.Generic;
using System.Linq;

namespace RoR2Randomizer.RandomizerControllers.ExplicitSpawn
{
    public static class ItemDescriptionNameReplacementManager
    {
        class Entry
        {
            public readonly string ItemPickupToken;
            public readonly BodyIndex SearchIndex;
            public readonly string SearchToken;
            public readonly BoolConfigValue IsEnabledConfigValue;

            LanguageAPI.LanguageOverlay[] _currentOverlays;

            public Entry(string itemPickupToken, BodyIndex searchIndex, BoolConfigValue isEnabledConfigValue)
            {
                const string LOG_PREFIX = $"{nameof(ItemDescriptionNameReplacementManager)}+{nameof(Entry)}..ctor ";

                ItemPickupToken = itemPickupToken;
                SearchIndex = searchIndex;

                CharacterBody bodyPrefab = BodyCatalog.GetBodyPrefabBodyComponent(searchIndex);
                if (bodyPrefab)
                {
                    SearchToken = bodyPrefab.baseNameToken;
                }
                else
                {
                    Log.Warning(LOG_PREFIX + $"null body prefab for search index {searchIndex}");
                    SearchToken = "UNKNOWN";
                }

                IsEnabledConfigValue = isEnabledConfigValue;
            }

            public void ApplyOverlays()
            {
                if (IsEnabledConfigValue != null && !IsEnabledConfigValue)
                    return;

                if (!ExplicitSpawnRandomizerController.TryGetReplacementBodyIndex(SearchIndex, out BodyIndex replacementIndex))
                    return;

                CharacterBody replacementBodyPrefab = BodyCatalog.GetBodyPrefabBodyComponent(replacementIndex);
                if (!replacementBodyPrefab)
                    return;

                string replacementNameToken = replacementBodyPrefab.baseNameToken;

                Language[] languages = Language.GetAllLanguages().ToArray();

                _currentOverlays = new LanguageAPI.LanguageOverlay[languages.Length];
                for (int i = 0; i < languages.Length; i++)
                {
                    string languageName = languages[i].name;

                    string basePickupText = Language.GetString(ItemPickupToken);
                    basePickupText = basePickupText.Replace(Language.GetString(SearchToken), Language.GetString(replacementNameToken));

                    _currentOverlays[i] = LanguageAPI.AddOverlay(ItemPickupToken, basePickupText, languageName);
                }
            }

            public void UndoOverlays()
            {
                if (_currentOverlays != null)
                {
                    foreach (LanguageAPI.LanguageOverlay overlay in _currentOverlays)
                    {
                        overlay?.Remove();
                    }

                    _currentOverlays = null;
                }
            }
        }

        static readonly List<Entry> _entries = new List<Entry>();

        [SystemInitializer]
        static void Init()
        {
            FullExplicitSpawnInitListener.OnFullInit += FullExplicitSpawnInitListener_OnFullInit;

            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        static void FullExplicitSpawnInitListener_OnFullInit()
        {
            foreach (Entry entry in _entries)
            {
                entry?.ApplyOverlays();
            }
        }

        static void Run_onRunDestroyGlobal(Run obj)
        {
            foreach (Entry entry in _entries)
            {
                entry?.UndoOverlays();
            }
        }

        public static void AddEntry(string itemPickupToken, BodyIndex searchIndex, BoolConfigValue isEnabledConfigValue)
        {
            _entries.Add(new Entry(itemPickupToken, searchIndex, isEnabledConfigValue));
        }
    }
}
