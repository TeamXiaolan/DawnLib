using InjectionLibrary.Attributes;

[assembly: RequiresInjections]
[assembly: HandleErrors(InjectionLibrary.ErrorHandlingStrategy.LogError)]

namespace Dawn.Interfaces;

[InjectInterface(typeof(EnemyAINestSpawnObject))]
[InjectInterface(typeof(RandomMapObject))]
[InjectInterface(typeof(PlaceableShipObject))]
[InjectInterface(typeof(HauntedMaskItem))]
[InjectInterface(typeof(RandomScrapSpawn))]
interface IAwakeMethod
{
    void Awake();
}