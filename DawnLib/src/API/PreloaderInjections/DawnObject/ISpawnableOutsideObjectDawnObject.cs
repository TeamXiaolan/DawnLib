using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(SpawnableOutsideObject))]
public interface ISpawnableOutsideObjectDawnObject
{
    DawnMapObjectInfo DawnInfo { get; set; }
}