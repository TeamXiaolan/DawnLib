using Dawn.Preloader;

namespace Dawn.Interfaces;

[InjectInterface("EnemyAINestSpawnObject")]
[InjectInterface("RandomMapObject")]
[InjectInterface("PlaceableShipObject")]
[InjectInterface("HauntedMaskItem")]
interface IAwakeMethod
{
    void Awake();
}