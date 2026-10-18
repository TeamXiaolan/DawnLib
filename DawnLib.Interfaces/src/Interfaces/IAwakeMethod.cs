using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

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