using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[HandleErrors(InjectionLibrary.ErrorHandlingStrategy.Ignore)]
[InjectInterface(typeof(EnemyAINestSpawnObject))]
[InjectInterface(typeof(RandomMapObject))]
[InjectInterface(typeof(PlaceableShipObject))]
[InjectInterface(typeof(HauntedMaskItem))]
interface IAwakeMethod
{
    [HandleErrors(InjectionLibrary.ErrorHandlingStrategy.Ignore)]
    void Awake();
}