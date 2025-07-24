using CodeRebirthLib.ContentManagement.Achievements;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

namespace CodeRebirthLib.Patches;

static class MenuManagerPatch
{
    internal static void Init()
    {
        On.MenuManager.Start += MenuManager_Start;
    }

    private static void MenuManager_Start(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);
        CRAchievementHandler.LoadAll();
        var canvas = GameObject.Instantiate(CodeRebirthLibPlugin.Main.AchievementUICanvasPrefab, self.transform.parent.Find("MenuContainer"));
        canvas.GetComponent<AchievementUICanvas>()._menuManager = self;
    }
}