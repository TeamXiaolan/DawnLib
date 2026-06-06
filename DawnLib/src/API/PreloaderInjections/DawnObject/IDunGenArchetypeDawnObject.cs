using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(DunGen.DungeonArchetype))]
public interface IDunGenArchetypeDawnObject
{
    DawnArchetypeInfo DawnInfo { get; set; }
}