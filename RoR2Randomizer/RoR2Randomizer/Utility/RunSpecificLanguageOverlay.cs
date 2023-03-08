using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.Utility
{
    public static class RunSpecificLanguageOverlay
    {
        static readonly List<LanguageAPI.LanguageOverlay> _activeOverlays = new List<LanguageAPI.LanguageOverlay>();

        static RunSpecificLanguageOverlay()
        {
            Run.onRunDestroyGlobal += onRunEnd;
        }

        static void onRunEnd(Run _)
        {
            foreach (LanguageAPI.LanguageOverlay overlay in _activeOverlays)
            {
                overlay.Remove();
            }

            _activeOverlays.Clear();
        }

        public static void AddRunLanguageOverlay(LanguageAPI.LanguageOverlay overlay)
        {
            _activeOverlays.Add(overlay);
        }
    }
}
