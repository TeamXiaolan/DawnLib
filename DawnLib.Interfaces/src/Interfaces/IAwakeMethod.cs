using Dawn.Preloader;

namespace Dawn.Interfaces;

[InjectInterface("EnemyAINestSpawnObject")]
[InjectInterface("RandomMapObject")]
[InjectInterface("PlaceableShipObject")]
interface IAwakeMethod
{
    void Awake();
}