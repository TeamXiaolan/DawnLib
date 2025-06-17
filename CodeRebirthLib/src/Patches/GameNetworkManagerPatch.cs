using CodeRebirthLib.Util;

namespace CodeRebirthLib.Patches;
static class GameNetworkManagerPatch
{
    internal static void Init()
    {
        On.GameNetworkManager.Start += CollectVanillaEnemies;
    }
    private static void CollectVanillaEnemies(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        VanillaEnemies.Init();
    }
}