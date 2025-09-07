namespace Dawn.Preloader.Interfaces;

[InjectInterface("SelectableLevel")]
[InjectInterface("WeatherEffect")]
[InjectInterface("EnemyType")]
[InjectInterface("Item")]
[InjectInterface("UnlockableItem")]
[InjectInterface("DunGen.TileSet")]
[InjectInterface("DunGen.DungeonArchetype")]
[InjectInterface("DunGen.Graph.DungeonFlow")]
[InjectInterface("BuyableVehicle")]
public interface IDawnObject
{
    object DawnInfo { get; set; }
}