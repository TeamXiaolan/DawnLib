namespace Dawn.Preloader.Interfaces;

[InjectInterface("EnemyAINestSpawnObject")]
[InjectInterface("RandomMapObject")]
interface IAwakeMethod
{
    void Awake();
}