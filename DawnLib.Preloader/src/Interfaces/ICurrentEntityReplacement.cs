namespace Dawn.Preloader.Interfaces;

[InjectInterface("EnemyType")]
[InjectInterface("Item")]
public interface ICurrentEntityReplacement
{
    object? CurrentEntityReplacement { get; set; }
}