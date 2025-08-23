namespace CodeRebirthLib.Utils;
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

    public static float GetCurrentProgress(this IProgress progress)
    {
        return progress.CurrentProgress;
    }

    public static float GetMaxProgress(this IProgress progress)
    {
        return progress.MaxProgress;
    }
}