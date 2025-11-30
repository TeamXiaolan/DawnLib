using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface("EnemyAI")]
[InjectInterface("GrabbableObject")]
[InjectInterface("EnemyAINestSpawnObject")]
public interface ICurrentEntityReplacement
{
    object? CurrentEntityReplacement { get; set; }
}