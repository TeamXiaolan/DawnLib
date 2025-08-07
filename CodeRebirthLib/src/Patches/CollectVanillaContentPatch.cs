using System.Collections;
using CodeRebirthLib.ContentManagement;

namespace CodeRebirthLib.Patches;

static class CollectVanillaContentPatch
{
    internal static void Init()
    {
        On.StartOfRound.Awake += StartOfRound_Awake;
        On.RoundManager.Awake += RoundManager_Awake;
        On.GameNetworkManager.Start += GameNetworkManagerOnStart;
    }

    private static void RoundManager_Awake(On.RoundManager.orig_Awake orig, RoundManager self)
    {
        orig(self);
        LethalContent.Dungeons.Init();
    }

    private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);
        LethalContent.Levels.Init();
    }

    private static void GameNetworkManagerOnStart(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);
        // delay by a frame because stuff wasnt getting picked up.
        self.StartCoroutine(GrabReferencesDelayed());
    }

    private static IEnumerator GrabReferencesDelayed()
    {
        yield return null;
        LethalContent.Enemies.Init();
        LethalContent.Items.Init();
    }
}