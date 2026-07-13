namespace Dawn;

public interface IWeightProfile
{
    void MarkDirty();
    void Rebuild(WeightBuildContext context);
}