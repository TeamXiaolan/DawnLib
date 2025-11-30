using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[HandleErrors(InjectionLibrary.ErrorHandlingStrategy.Ignore)]
[InjectInterface("EnemyAINestSpawnObject")]
[InjectInterface("RandomMapObject")]
[InjectInterface("PlaceableShipObject")]
[InjectInterface("HauntedMaskItem")]
interface IAwakeMethod
{
    [HandleErrors(InjectionLibrary.ErrorHandlingStrategy.Ignore)]
    void Awake();
}