namespace CodeRebirthLib;
public interface IWeightProvider<in TBase> where TBase : INamespaced<TBase>
{
    bool IsActive(TBase info);
    int GetWeight(TBase key);
}