using Dawn.Preloader;

namespace Dawn.Interfaces;

[InjectInterface("SelectableLevel")]
[InjectInterface("WeatherEffect")]
[InjectInterface("EnemyType")]
[InjectInterface("Item")]
[InjectInterface("UnlockableItem")]
[InjectInterface("TileSet")]
[InjectInterface("DungeonArchetype")]
[InjectInterface("DungeonFlow")]
[InjectInterface("BuyableVehicle")]
public interface IDawnObject
{
    object DawnInfo { get; set; }
}