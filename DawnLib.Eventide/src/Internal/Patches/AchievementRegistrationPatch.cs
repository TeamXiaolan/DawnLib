using Eventide.Utils;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace Eventide.Internal;

static class AchievementRegistrationPatch
{
    internal static void Init()
    {
        using (new DetourContext(priority: 9999))
        {
            On.StartOfRound.Awake += CreateAchievementUI;
        }

        On.GameNetworkManager.SaveLocalPlayerValues += SaveAchievementData;
        On.MenuManager.Start += LoadAchievementDataWithUI;
        On.StartOfRound.AutoSaveShipData += SaveAchievementData;
    }

    private static void CreateAchievementUI(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        if (AchievementUIGetCanvas.Instance == null)
        {
            Object.Instantiate(EventidePlugin.EventideMain.AchievementGetUICanvasPrefab);
        }

        orig(self);
    }

    private static void SaveAchievementData(On.StartOfRound.orig_AutoSaveShipData orig, StartOfRound self)
    {
        orig(self);
        EventideAchievementHandler.SaveAll();
    }

    private static void LoadAchievementDataWithUI(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);
        if (EventideModContent.Achievements.Count == 0 || EventideConfig.DisableAchievementsButton.Value)
            return;

        EventideAchievementHandler.LoadAll();
        DoAchievementUI(self);
    }

    private static void DoAchievementUI(MenuManager menuManager)
    {
        var canvas = GameObject.Instantiate(EventidePlugin.EventideMain.AchievementUICanvasPrefab, menuManager.transform.parent.Find("MenuContainer"));
        canvas.GetComponent<AchievementUICanvas>()._menuManager = menuManager;

        if (AchievementUIGetCanvas.Instance == null)
        {
            Object.Instantiate(EventidePlugin.EventideMain.AchievementGetUICanvasPrefab);
        }

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
        EventideAchievementHandler.SaveAll();
    }
}