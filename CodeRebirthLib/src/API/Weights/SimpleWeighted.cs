namespace CodeRebirthLib;
public class SimpleWeighted(int weight) : IWeighted
{
    public int GetWeight() => weight;
}

public class SimpleWeightProvider<TBase>(IWeighted weight) : IProvider<int?, TBase> where TBase : INamespaced<TBase>
{

    public int? Provide(TBase info) => weight.GetWeight();
}

public class SimpleProvider<T, TBase>(T value) : IProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T Provide(TBase info) => value;
}
public class MatchingKeyProvider<T, TBase>(NamespacedKey<TBase> targetKey, T value) : IProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T? Provide(TBase info)
    {
        return Equals(targetKey) ? value : default;
    }
}

public class HasTagProvider<T, TBase>(NamespacedKey tag, T value) : IProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T? Provide(TBase info)
    {
        if (info is not ITaggable taggable) return default;
        if (!taggable.HasTag(tag)) return default;
        return value;
    }
}

public class MatchingKeyWeightProvider<TBase>(NamespacedKey<TBase> targetKey, IWeighted weight) : IProvider<int?, TBase> where TBase : INamespaced<TBase>
{

    public int? Provide(TBase info)
    {
        return Equals(targetKey) ? weight.GetWeight() : null;
    }
}

public class HasTagWeightProvider<TBase>(NamespacedKey tag, IWeighted weight) : IProvider<int?, TBase> where TBase : INamespaced<TBase>
{
    public int? Provide(TBase info)
    {
        if (info is not ITaggable taggable) return null;
        if (!taggable.HasTag(tag)) return null;
        return weight.GetWeight();
    }
}