using CodeRebirthLib.ContentManagement.Enemies;
using CodeRebirthLib.Util;

namespace CodeRebirthLib.Patches;

static class GameNetworkManagerPatch
{
    internal static void Init()
    {
        On.GameNetworkManager.Start += GameNetworkManagerOnStart;
        On.GameNetworkManager.SaveItemsInShip += GameNetworkManager_SaveItemsInShip;
        On.GameNetworkManager.ResetSavedGameValues += GameNetworkManager_ResetSavedGameValues;
    }

    private static void GameNetworkManagerOnStart(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        VanillaEnemies.Init();
    }

    private static void GameNetworkManager_SaveItemsInShip(On.GameNetworkManager.orig_SaveItemsInShip orig, GameNetworkManager self)
    {
        orig(self);
        CodeRebirthLibPlugin.ExtendedLogging("Saving CodeRebirthLibData");
        CodeRebirthLibNetworker.Instance?.SaveCodeRebirthLibData();
    }

    private static void GameNetworkManager_ResetSavedGameValues(On.GameNetworkManager.orig_ResetSavedGameValues orig, GameNetworkManager self)
    {
        orig(self);
        ES3Settings settings;
        if (CodeRebirthLibNetworker.Instance != null)
        {
            settings = CodeRebirthLibNetworker.Instance.SaveSettings;
        }
        else
        {
            settings = new ES3Settings($"CRLib{GameNetworkManager.Instance.currentSaveFileName}", ES3.EncryptionType.None);
        }
        CodeRebirthLibNetworker.ResetCodeRebirthLibData(settings);
    }
}