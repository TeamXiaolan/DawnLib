using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(EnemyAI))]
[InjectInterface(typeof(GrabbableObject))]
[InjectInterface(typeof(EnemyAINestSpawnObject))]
public interface ICurrentEntityReplacement
{
    object? CurrentEntityReplacement { get; set; }
}