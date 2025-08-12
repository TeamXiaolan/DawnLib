using UnityEngine;

namespace CodeRebirthLib;

static class AchievementRegistrationHandler
{
    internal static void Init()
    {
        On.GameNetworkManager.SaveLocalPlayerValues += SaveAchievementData;
        On.MenuManager.Start += LoadAchievementDataWithUI;
        On.StartOfRound.AutoSaveShipData += SaveAchievementData;
    }

    private static void SaveAchievementData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        CRAchievementHandler.SaveAll();
    }

    private static void LoadAchievementDataWithUI(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);
        CRAchievementHandler.LoadAll();
        DoAchievementUI(self);
    }

    private static void DoAchievementUI(MenuManager menuManager)
    {
        var canvas = GameObject.Instantiate(CodeRebirthLibPlugin.Main.AchievementUICanvasPrefab, menuManager.transform.parent.Find("MenuContainer"));
        canvas.GetComponent<AchievementUICanvas>()._menuManager = menuManager;

        if (AchievementUIGetCanvas.Instance == null)
            Object.Instantiate(CodeRebirthLibPlugin.Main.AchievementGetUICanvasPrefab);

        var menuContainer = GameObject.Find("MenuContainer");
        if (!menuContainer)
            return;

        var mainButtonsTransform = menuContainer.transform.Find("MainButtons");
        if (!mainButtonsTransform)
            return;

        var quitButton = mainButtonsTransform.Find("QuitButton");
        if (!quitButton)
            return;

        MenuUtils.InjectMenu(mainButtonsTransform, quitButton.gameObject);
    }

    private static void SaveAchievementData(On.GameNetworkManager.orig_SaveLocalPlayerValues orig, GameNetworkManager self)
    {
        orig(self);
        CRAchievementHandler.SaveAll();
    }
}