using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[HandleErrors(InjectionLibrary.ErrorHandlingStrategy.Ignore)]
[InjectInterface(typeof(PlaceableShipObject))]
[InjectInterface(typeof(EntranceTeleport))]
interface IOnDestroyMethod
{
    [HandleErrors(InjectionLibrary.ErrorHandlingStrategy.Ignore)]
    void OnDestroy();
}