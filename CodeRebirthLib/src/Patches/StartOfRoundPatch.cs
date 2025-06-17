using CodeRebirthLib.ContentManagement.Enemies;

namespace CodeRebirthLib.Patches;
static class StartOfRoundPatch
{
    internal static void Init()
    {
        On.StartOfRound.Start += StartOfRoundOnStart;
        On.StartOfRound.SetPlanetsWeather += RefreshEnemyWeights;
    }
    private static void RefreshEnemyWeights(On.StartOfRound.orig_SetPlanetsWeather orig, StartOfRound self, int connectedplayersonserver)
    {
        orig(self, connectedplayersonserver);
        CREnemyDefinition.UpdateAllWeights();
    }
    private static void StartOfRoundOnStart(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        orig(self);
        CREnemyDefinition.CreateMoonAttributeStacks();
    }
}