namespace Dawn.Preloader.Interfaces;

[InjectInterface("EnemyAINestSpawnObject")]
[InjectInterface("RandomMapObject")]
[InjectInterface("PlaceableShipObject")]
interface IAwakeMethod
{
    void Awake();
}