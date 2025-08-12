using UnityEngine;

namespace CodeRebirthLib.Utils;
public static class MoreLayerMasks
{
    public static int CollidersAndRoomAndDefaultAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleAndDefaultMask { get; private set; }
    public static int CollidersAndRoomAndRailingAndTerrainAndHazardAndVehicleAndDefaultMask { get; private set; }
    public static int CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndRailingAndInteractableMask { get; private set; }
    public static int CollidersAndRoomAndPlayersAndInteractableMask { get; private set; }
    public static int PlayersAndInteractableAndEnemiesAndPropsHazardMask { get; private set; }
    public static int CollidersAndRoomMaskAndDefaultAndEnemies { get; private set; }
    public static int PlayersAndEnemiesAndHazardMask { get; private set; }
    public static int PlayersAndRagdollMask { get; private set; }
    public static int PropsAndHazardMask { get; private set; }
    public static int TerrainAndFoliageMask { get; private set; }
    public static int PlayersAndEnemiesMask { get; private set; }
    public static int DefaultMask { get; private set; }
    public static int PropsMask { get; private set; }
    public static int HazardMask { get; private set; }
    public static int EnemiesMask { get; private set; }
    public static int InteractableMask { get; private set; }
    public static int RailingMask { get; private set; }
    public static int TerrainMask { get; private set; }
    public static int VehicleMask { get; private set; }

    public static void Init()
    {
        DefaultMask = LayerMask.GetMask("Default");
        PropsMask = LayerMask.GetMask("Props");
        HazardMask = LayerMask.GetMask("MapHazards");
        EnemiesMask = LayerMask.GetMask("Enemies");
        InteractableMask = LayerMask.GetMask("InteractableObject");
        RailingMask = LayerMask.GetMask("Railing");
        TerrainMask = LayerMask.GetMask("Terrain");
        VehicleMask = LayerMask.GetMask("Vehicle");
        PlayersAndRagdollMask = StartOfRound.Instance.playersMask | LayerMask.GetMask("PlayerRagdoll");
        PropsAndHazardMask = PropsMask | HazardMask;
        TerrainAndFoliageMask = TerrainMask | LayerMask.GetMask("Foliage");
        PlayersAndEnemiesMask = StartOfRound.Instance.playersMask | EnemiesMask;
        PlayersAndEnemiesAndHazardMask = PlayersAndEnemiesMask | HazardMask;
        CollidersAndRoomMaskAndDefaultAndEnemies = StartOfRound.Instance.collidersAndRoomMaskAndDefault | EnemiesMask;
        CollidersAndRoomAndRailingAndInteractableMask = StartOfRound.Instance.collidersAndRoomMask | InteractableMask | RailingMask;
        CollidersAndRoomAndPlayersAndInteractableMask = StartOfRound.Instance.collidersAndRoomMaskAndPlayers | InteractableMask;
        PlayersAndInteractableAndEnemiesAndPropsHazardMask = PlayersAndEnemiesAndHazardMask | InteractableMask | PropsMask;
        CollidersAndRoomAndRailingAndTerrainAndHazardAndVehicleAndDefaultMask = StartOfRound.Instance.collidersAndRoomMask | HazardMask | RailingMask | TerrainMask | VehicleMask | DefaultMask;
        CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask = StartOfRound.Instance.collidersAndRoomMaskAndPlayers | EnemiesMask | TerrainMask | VehicleMask;
        CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleAndDefaultMask = CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask | DefaultMask;
        CollidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask = CollidersAndRoomAndRailingAndInteractableMask | HazardMask | TerrainMask | VehicleMask;
        CollidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = CollidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask | EnemiesMask;
        CollidersAndRoomAndDefaultAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = CollidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask | DefaultMask;
    }
}