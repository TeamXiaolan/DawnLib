using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(DunGen.Graph.DungeonFlow))]
public interface IDunGenFlowDawnObject
{
    DawnDungeonInfo DawnInfo { get; set; }
}