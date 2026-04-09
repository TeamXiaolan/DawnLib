using UnityEngine;

namespace Dawn.Utils;

public static class MoreLayerMasks
{
    public static int CollidersAndRoomAndDefaultAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask { get; private set; }
    public static int RoomAndPlayerAndAndEnemiesAndTerrainAndHazardAndVehicleAndPropsAndDefaultMask { get; private set; }
    public static int CollidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleAndDefaultMask { get; private set; }
    public static int CollidersAndRoomAndRailingAndTerrainAndHazardAndVehicleAndDefaultMask { get; private set; }
    public static int CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask { get; private set; }
    public static int CollidersAndRoomAndRailingAndInteractableMask { get; private set; }
    public static int CollidersAndRoomAndPlayersAndInteractableMask { get; private set; }
    public static int PlayersAndInteractableAndEnemiesAndPropsHazardMask { get; private set; }
    public static int CollidersAndRoomMaskAndDefaultAndEnemies { get; private set; }
    public static int PlayersAndEnemiesAndHazardMask { get; private set; }
    public static int DefaultRoomAndNavigationSurfaceMask { get; private set; }
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
    public static int PlayerMask { get; private set; }
    public static int CollidersMask { get; private set; }
    public static int RoomMask { get; private set; }
    public static int NavigationSurfaceMask { get; private set; }
    public static int FoliageMask { get; private set; }
    public static int PlayerRagdollMask { get; private set; }

    public static void Init()
    {
        // One
        DefaultMask = LayerMask.GetMask("Default");
        PropsMask = LayerMask.GetMask("Props");
        HazardMask = LayerMask.GetMask("MapHazards");
        EnemiesMask = LayerMask.GetMask("Enemies");
        InteractableMask = LayerMask.GetMask("InteractableObject");
        RailingMask = LayerMask.GetMask("Railing");
        TerrainMask = LayerMask.GetMask("Terrain");
        VehicleMask = LayerMask.GetMask("Vehicle");
        PlayerMask = LayerMask.GetMask("Player");
        CollidersMask = LayerMask.GetMask("Colliders");
        RoomMask = LayerMask.GetMask("Room");
        NavigationSurfaceMask = LayerMask.GetMask("NavigationSurface");
        FoliageMask = LayerMask.GetMask("Foliage");
        PlayerRagdollMask = LayerMask.GetMask("PlayerRagdoll");

        // Two
        PlayersAndRagdollMask = PlayerMask | PlayerRagdollMask;
        PropsAndHazardMask = PropsMask | HazardMask;
        TerrainAndFoliageMask = TerrainMask | FoliageMask;
        PlayersAndEnemiesMask = PlayerMask | EnemiesMask;

        // Three
        PlayersAndEnemiesAndHazardMask = PlayersAndEnemiesMask | HazardMask;
        DefaultRoomAndNavigationSurfaceMask = DefaultMask | RoomMask | NavigationSurfaceMask;

        // Four
        CollidersAndRoomMaskAndDefaultAndEnemies = DefaultMask | CollidersMask | RoomMask | EnemiesMask;
        CollidersAndRoomAndRailingAndInteractableMask = CollidersMask | RoomMask | InteractableMask | RailingMask;
        CollidersAndRoomAndPlayersAndInteractableMask = CollidersMask | RoomMask | PlayerMask | InteractableMask;

        // Five
        PlayersAndInteractableAndEnemiesAndPropsHazardMask = PlayersAndEnemiesAndHazardMask | InteractableMask | PropsMask;

        // Six
        CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask = CollidersMask | RoomMask | PlayerMask | EnemiesMask | TerrainMask | VehicleMask;

        // Seven
        CollidersAndRoomAndRailingAndTerrainAndHazardAndVehicleAndDefaultMask = CollidersMask | RoomMask | HazardMask | RailingMask | TerrainMask | VehicleMask | DefaultMask;
        CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleAndDefaultMask = CollidersAndRoomAndPlayersAndEnemiesAndTerrainAndVehicleMask | DefaultMask;
        CollidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask = CollidersAndRoomAndRailingAndInteractableMask | HazardMask | TerrainMask | VehicleMask;

        // Eight
        CollidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = CollidersAndRoomAndInteractableAndRailingAndTerrainAndHazardAndVehicleMask | EnemiesMask;
        RoomAndPlayerAndAndEnemiesAndTerrainAndHazardAndVehicleAndPropsAndDefaultMask = RoomMask | PlayerMask | EnemiesMask | TerrainMask | HazardMask | VehicleMask | PropsMask | DefaultMask;

        // Nine
        CollidersAndRoomAndDefaultAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask = CollidersAndRoomAndInteractableAndRailingAndEnemiesAndTerrainAndHazardAndVehicleMask | DefaultMask;
    }
}