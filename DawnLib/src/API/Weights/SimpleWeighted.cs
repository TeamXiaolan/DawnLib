namespace Dawn;
public class SimpleWeighted(int weight) : IWeighted
{
    public int GetWeight() => weight;
}

public interface IContextualWeighted<in TContext>
{
    int GetWeight(TContext ctx);
}

public static class WeightedExtensions
{
    public static int GetWeight<TContext>(this IWeighted weighted, in TContext ctx)
    {
        if (weighted is IContextualWeighted<TContext> contextual)
        {
            return contextual.GetWeight(ctx);
        }

        return weighted.GetWeight();
    }
}

public class SimpleContextualProvider<T, TBase>(T value) : IContextualProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T Provide(TBase info) => value;
}

public class MatchingKeyContextualProvider<T, TBase>(NamespacedKey<TBase> targetKey, T value) : IContextualProvider<T, TBase> where TBase : INamespaced<TBase>
{
    public T? Provide(TBase info)
    {
        return Equals(info.Key, targetKey) ? value : default;
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

public sealed class MatchingKeyWeightContextualProvider<TBase, TContext>(NamespacedKey<TBase> targetKey, IWeighted weight) : IContextualProvider<int?, TBase, TContext> where TBase : INamespaced<TBase>
{
    public int? Provide(TBase info, TContext ctx) => Equals(info.Key, targetKey) ? weight.GetWeight(ctx) : null;
}

public sealed class HasTagWeightContextualProvider<TBase, TContext>(NamespacedKey tag, IWeighted weight) : IContextualProvider<int?, TBase, TContext> where TBase : INamespaced<TBase>, ITaggable
{
    public int? Provide(TBase info, TContext ctx)
    {
        if (!info.HasTag(tag))
            return null;

        return weight.GetWeight(ctx);
    }
}

public sealed class SimpleWeightContextualProvider<TBase, TContext>(IWeighted weight) : IContextualProvider<int?, TBase, TContext> where TBase : INamespaced<TBase>
{
    public int? Provide(TBase info, TContext ctx) => weight.GetWeight(ctx);
}
