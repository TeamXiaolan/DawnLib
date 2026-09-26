using InjectionLibrary.Attributes;

[assembly: RequiresInjections]
[assembly: HandleErrors(InjectionLibrary.ErrorHandlingStrategy.LogError)]

namespace Dawn.Interfaces;

[InjectInterface(typeof(PlaceableShipObject))]
interface IOnDestroyMethod
{
    void OnDestroy();
}