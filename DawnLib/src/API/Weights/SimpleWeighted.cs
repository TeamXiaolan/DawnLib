namespace Dawn;
public class SimpleWeighted(int weight) : IWeighted
{
    public int GetWeight() => weight;
}

public class SimpleWeightContextualProvider<TBase>(IWeighted weight) : IContextualProvider<int?, TBase> where TBase : INamespaced<TBase>
{

    public int? Provide(TBase info) => weight.GetWeight();
}

public class SimpleContextualProvider<T, TBase>(T value) : IContextualProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T Provide(TBase info) => value;
}
public class MatchingKeyContextualProvider<T, TBase>(NamespacedKey<TBase> targetKey, T value) : IContextualProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T? Provide(TBase info)
    {
        return Equals(targetKey) ? value : default;
    }
}

public class HasTagContextualProvider<T, TBase>(NamespacedKey tag, T value) : IContextualProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T? Provide(TBase info)
    {
        if (info is not ITaggable taggable) return default;
        if (!taggable.HasTag(tag)) return default;
        return value;
    }
}

public class MatchingKeyWeightContextualProvider<TBase>(NamespacedKey<TBase> targetKey, IWeighted weight) : IContextualProvider<int?, TBase> where TBase : INamespaced<TBase>
{

    public int? Provide(TBase info)
    {
        return Equals(targetKey) ? weight.GetWeight() : null;
    }
}

public class HasTagWeightContextualProvider<TBase>(NamespacedKey tag, IWeighted weight) : IContextualProvider<int?, TBase> where TBase : INamespaced<TBase>
{
    public int? Provide(TBase info)
    {
        if (info is not ITaggable taggable) return null;
        if (!taggable.HasTag(tag)) return null;
        return weight.GetWeight();
    }
}