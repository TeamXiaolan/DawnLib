namespace CodeRebirthLib.Data;
public interface IProgress
{
    float MaxProgress { get; }
    float CurrentProgress { get; }
}

public static class IProgressExtensions
{
    public static float Percentage(this IProgress progress)
    {
        return progress.CurrentProgress / progress.MaxProgress;
    }
}