using BepInEx.Bootstrap;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.ModCompatibility
{
    public static class RiskOfOptionsCompat
    {
        public static readonly InitializeOnAccess<bool> IsEnabled = new InitializeOnAccess<bool>(() => Chainloader.PluginInfos.ContainsKey(Constants.RISK_OF_OPTIONS_GUID));

        static readonly InitializeOnAccess<Sprite> _iconSprite = new InitializeOnAccess<Sprite>(() =>
        {
            const int IMAGE_SIZE = 256;

            Texture2D texture2D = new Texture2D(IMAGE_SIZE, IMAGE_SIZE);
            if (texture2D.LoadImage(Properties.Resources.icon_embed))
            {
                return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
            }
            else
            {
                Log.Warning($"{nameof(RiskOfOptionsCompat)} LoadImage failed");
            }

            return null;
        });

        public static void Setup()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("A Risk of Rain 2 randomizer mod.\n\nMost of the settings will only take effect when the next run is started.");

            Sprite sprite = _iconSprite;
            if (sprite)
            {
                RiskOfOptions.ModSettingsManager.SetModIcon(sprite);
            }
        }
    }
}
