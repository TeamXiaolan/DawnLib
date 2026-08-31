using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(PlaceableShipObject))]
interface IOnDestroyMethod
{
    void OnDestroy();
}