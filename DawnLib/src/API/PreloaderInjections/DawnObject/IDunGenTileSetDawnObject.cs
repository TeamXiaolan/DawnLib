using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(DunGen.TileSet))]
public interface IDunGenTileSetDawnObject
{
    DawnTileSetInfo DawnInfo { get; set; }
}