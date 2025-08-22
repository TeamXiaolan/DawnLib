namespace CodeRebirthLib.Preloader;

[InjectInterface("SelectableLevel")]
[InjectInterface("WeatherEffect")]
[InjectInterface("EnemyType")]
[InjectInterface("Item")]
[InjectInterface("UnlockableItem")]
[InjectInterface("DunGen.TileSet")]
[InjectInterface("DunGen.Graph.DunGenFlow")]
public interface ICRObject
{
    object CRInfo { get; set; }
}