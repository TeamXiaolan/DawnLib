using CodeRebirthLib.ContentManagement;

namespace CodeRebirthLib.Patches;
static class CollectVanillaContentPatch
{
    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.GameNetworkManager.Start += GameNetworkManagerOnStart;
    }
    
    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        LethalContent.Levels.Init();
        LethalContent.Dungeons.Init();
    }
    private static void GameNetworkManagerOnStart(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        LethalContent.Enemies.Init();
        LethalContent.Items.Init();
    }
}