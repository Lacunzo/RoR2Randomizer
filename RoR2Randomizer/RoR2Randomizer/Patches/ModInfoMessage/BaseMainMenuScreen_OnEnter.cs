using RoR2.UI;
using RoR2.UI.MainMenu;
using RoR2Randomizer.Configuration;

namespace RoR2Randomizer.Patches.ModInfoMessage
{
    [PatchClass]
    static class ShowModInfoMessagePatch
    {
        static bool _isShowingModInfo;

        static void Apply()
        {
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += BaseMainMenuScreen_OnEnter;
        }

        static void Cleanup()
        {
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter -= BaseMainMenuScreen_OnEnter;
        }

        static void BaseMainMenuScreen_OnEnter(On.RoR2.UI.MainMenu.BaseMainMenuScreen.orig_OnEnter orig, BaseMainMenuScreen self, MainMenuController mainMenuController)
        {
            if (mainMenuController && self == mainMenuController.titleMenuScreen && ConfigManager.Metadata.DisplayModInfoOnStart && !_isShowingModInfo)
            {
                _isShowingModInfo = true;

                SimpleDialogBox dialogBox = SimpleDialogBox.Create();

                dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair(LanguageTokens.MOD_INFO_POPUP_HEADER);
                dialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair(LanguageTokens.MOD_INFO_POPUP_DESCRIPTION);
                dialogBox.AddActionButton(() =>
                {
                    ConfigManager.Metadata.DisplayModInfoOnStart.Entry.Value = false;
                    _isShowingModInfo = false;
                }, LanguageTokens.MOD_INFO_POPUP_DISMISS);
            }

            orig(self, mainMenuController);
        }
    }
}
