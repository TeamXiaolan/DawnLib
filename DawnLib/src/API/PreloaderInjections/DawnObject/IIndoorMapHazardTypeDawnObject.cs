using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(IndoorMapHazardType))]
public interface IIndoorMapHazardTypeDawnObject
{
    DawnMapObjectInfo DawnInfo { get; set; }
}