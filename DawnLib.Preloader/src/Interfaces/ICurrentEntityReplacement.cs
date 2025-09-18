namespace Dawn.Preloader.Interfaces;

[InjectInterface("EnemyAI")]
[InjectInterface("GrabbableObject")]
[InjectInterface("EnemyAINestSpawnObject")]
public interface ICurrentEntityReplacement
{
    object? CurrentEntityReplacement { get; set; }
}