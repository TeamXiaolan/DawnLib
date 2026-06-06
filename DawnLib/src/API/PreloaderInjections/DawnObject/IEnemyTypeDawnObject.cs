using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(EnemyType))]
public interface IEnemyTypeDawnObject
{
    DawnEnemyInfo DawnInfo { get; set; }
}