using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(BuyableVehicle))]
public interface IBuyableVehicleDawnObject
{
    object DawnInfo { get; set; }
}