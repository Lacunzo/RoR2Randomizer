using R2API;
using RoR2;
using System.Collections.Generic;

namespace RoR2Randomizer
{
    public static class LanguageTokens
    {
        const string TOKEN_PREFIX = Main.PluginGUID + "_";

        public const string MOD_INFO_POPUP_HEADER = TOKEN_PREFIX + nameof(MOD_INFO_POPUP_HEADER);
        public const string MOD_INFO_POPUP_DESCRIPTION = TOKEN_PREFIX + nameof(MOD_INFO_POPUP_DESCRIPTION);
        public const string MOD_INFO_POPUP_DISMISS = TOKEN_PREFIX + nameof(MOD_INFO_POPUP_DISMISS);

        // TODO: Properly read this from a file of some kind
        static readonly Dictionary<string, string> _tokenPairs = new Dictionary<string, string>
        {
            // Body names
            { "ARCHWISP_BODY_NAME", "Arch Wisp" },

            { "BEETLE_CRYSTAL_BODY_NAME", "Crystal Beetle" },

            { "MAJORCONSTRUCT_BODY_NAME", "Major Construct" },
            { "MAJORCONSTRUCT_BODY_SUBTITLE", "Defense System" },

            // Mod info message
            { MOD_INFO_POPUP_HEADER, "Welcome to Risk of Rain 2 Randomizer!" },
            { MOD_INFO_POPUP_DESCRIPTION, "Thank you for wanting to play with my silly little mod! Before you start playing though, please check the mod settings (Settings -> Mod Options) if you don't want absolute chaos (almost all randomizers are enabled by default), or don't, up to you :)" },
            { MOD_INFO_POPUP_DISMISS, "OK" }
        };

        [SystemInitializer]
        static void Init()
        {
            LanguageAPI.Add(_tokenPairs);
        }
    }
}
