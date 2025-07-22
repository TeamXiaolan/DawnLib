using CodeRebirthLib.ContentManagement.Achievements;
using UnityEngine;

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
        GameObject.Instantiate(CodeRebirthLibPlugin.Main.AchievementUICanvasPrefab);
    }
}