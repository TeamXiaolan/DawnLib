using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(SelectableLevel))]
[InjectInterface(typeof(WeatherEffect))]
[InjectInterface(typeof(EnemyType))]
[InjectInterface(typeof(Item))]
[InjectInterface(typeof(UnlockableItem))]
[InjectInterface(typeof(DunGen.TileSet))]
[InjectInterface(typeof(DunGen.DungeonArchetype))]
[InjectInterface(typeof(DunGen.Graph.DungeonFlow))]
[InjectInterface(typeof(BuyableVehicle))]
[InjectInterface(typeof(FootstepSurface))]
public interface IDawnObject
{
    object DawnInfo { get; set; }
}