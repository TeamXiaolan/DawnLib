using UnityEngine;

namespace CodeRebirthLib.Util;
public static class MoreLayerMasks
{
    internal static int collidersAndRoomAndDefaultAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = 0;
    internal static int collidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = 0;
    internal static int collidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask = 0;
    internal static int collidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleAndDefaultMask = 0;
    internal static int collidersAndRoomAndRailingAndTerrainAndHazardAndVehicleAndDefaultMask = 0;
    internal static int collidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask = 0;
    internal static int collidersAndRoomAndRailingAndInteractableMask = 0;
    internal static int collidersAndRoomAndPlayersAndInteractableMask = 0;
    internal static int playersAndInteractableAndEnemiesAndPropsHazardMask = 0;
    internal static int collidersAndRoomMaskAndDefaultAndEnemies = 0;
    internal static int playersAndEnemiesAndHazardMask = 0;
    internal static int playersAndRagdollMask = 0;
    internal static int propsAndHazardMask = 0;
    internal static int terrainAndFoliageMask = 0;
    internal static int playersAndEnemiesMask = 0;
    internal static int defaultMask = 0;
    internal static int propsMask = 0;
    internal static int hazardMask = 0;
    internal static int enemiesMask = 0;
    internal static int interactableMask = 0;
    internal static int railingMask = 0;
    internal static int terrainMask = 0;
    internal static int vehicleMask = 0;

    public static void Init()
    {
        defaultMask = LayerMask.GetMask("Default");
        propsMask = LayerMask.GetMask("Props");
        hazardMask = LayerMask.GetMask("MapHazards");
        enemiesMask = LayerMask.GetMask("Enemies");
        interactableMask = LayerMask.GetMask("InteractableObject");
        railingMask = LayerMask.GetMask("Railing");
        terrainMask = LayerMask.GetMask("Terrain");
        vehicleMask = LayerMask.GetMask("Vehicle");
        playersAndRagdollMask = StartOfRound.Instance.playersMask | LayerMask.GetMask("PlayerRagdoll");
        propsAndHazardMask = propsMask | hazardMask;
        terrainAndFoliageMask = terrainMask | LayerMask.GetMask("Foliage");
        playersAndEnemiesMask = StartOfRound.Instance.playersMask | enemiesMask;
        playersAndEnemiesAndHazardMask = playersAndEnemiesMask | hazardMask;
        collidersAndRoomMaskAndDefaultAndEnemies = StartOfRound.Instance.collidersAndRoomMaskAndDefault | enemiesMask;
        collidersAndRoomAndRailingAndInteractableMask = StartOfRound.Instance.collidersAndRoomMask | interactableMask | railingMask;
        collidersAndRoomAndPlayersAndInteractableMask = StartOfRound.Instance.collidersAndRoomMaskAndPlayers | interactableMask;
        playersAndInteractableAndEnemiesAndPropsHazardMask = playersAndEnemiesAndHazardMask | interactableMask | propsMask;
        collidersAndRoomAndRailingAndTerrainAndHazardAndVehicleAndDefaultMask = StartOfRound.Instance.collidersAndRoomMask | hazardMask | railingMask | terrainMask | vehicleMask | defaultMask;
        collidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask = StartOfRound.Instance.collidersAndRoomMaskAndPlayers | enemiesMask | terrainMask | vehicleMask;
        collidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleAndDefaultMask = collidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask | defaultMask;
        collidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask = collidersAndRoomAndRailingAndInteractableMask | hazardMask | terrainMask | vehicleMask;
        collidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = collidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask | enemiesMask;
        collidersAndRoomAndDefaultAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = collidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask | defaultMask;
    }
}