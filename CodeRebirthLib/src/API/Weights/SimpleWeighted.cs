namespace CodeRebirthLib;
public class SimpleWeighted(int weight) : IWeighted
{
    public int GetWeight() => weight;
}

public class SimpleWeightProvider<TBase>(IWeighted weight) : IWeightProvider<TBase> where TBase : INamespaced<TBase>
{

    public bool IsActive(TBase info) => true;
    public int GetWeight(TBase key) => weight.GetWeight();
}

public class MatchingKeyWeightProvider<TBase>(NamespacedKey<TBase> targetKey, IWeighted weight) : IWeightProvider<TBase> where TBase : INamespaced<TBase>
{

    public bool IsActive(TBase info) => Equals(targetKey, info);
    public int GetWeight(TBase key)
    {
        return weight.GetWeight();
    }
}

public class HasTagWeightProvider<TBase>(NamespacedKey tag, IWeighted weight) : IWeightProvider<TBase> where TBase : INamespaced<TBase>
{

    public bool IsActive(TBase info)
    {
        if (info is not ITaggable taggable) return false;
        return taggable.HasTag(tag);
    }
    public int GetWeight(TBase key)
    {
        return weight.GetWeight();
    }
}