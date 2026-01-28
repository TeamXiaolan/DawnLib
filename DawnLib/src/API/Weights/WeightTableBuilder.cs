using System.Collections.Generic;

namespace Dawn;
public class WeightTableBuilder<TBase, TContext> where TBase : INamespaced<TBase>, ITaggable
{
    private List<IContextualProvider<int?, TBase, TContext>> _baseProviders = [];
    private List<IContextualProvider<int?, TBase, TContext>> _tagProviders = [];
    private IContextualProvider<int?, TBase, TContext>? _global;
    public WeightTableBuilder<TBase, TContext> AddWeight(NamespacedKey<TBase> key, int weight)
    {
        return AddWeight(key, new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase, TContext> AddWeight(NamespacedKey<TBase> key, IWeighted weight)
    {
        _baseProviders.Add(new MatchingKeyWeightContextualProvider<TBase, TContext>(key, weight));
        return this;
    }

    public WeightTableBuilder<TBase, TContext> AddTagWeight(NamespacedKey tag, int weight)
    {
        return AddTagWeight(tag, new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase, TContext> AddTagWeight(NamespacedKey tag, IWeighted weight)
    {
        _tagProviders.Add(new HasTagWeightContextualProvider<TBase, TContext>(tag, weight));
        return this;
    }

    public WeightTableBuilder<TBase, TContext> SetGlobalWeight(int weight)
    {
        return SetGlobalWeight(new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase, TContext> SetGlobalWeight(IWeighted weight)
    {
        return SetGlobalWeight(new SimpleWeightContextualProvider<TBase, TContext>(weight));
    }

    public WeightTableBuilder<TBase, TContext> SetGlobalWeight(IContextualProvider<int?, TBase, TContext> provider)
    {
        _global = provider;
        return this;
    }

    public ProviderTable<int?, TBase, TContext> Build()
    {
        List<IContextualProvider<int?, TBase, TContext>> compiled = [.. _baseProviders, .. _tagProviders];
        if (_global != null)
        {
            compiled.Add(_global);
        }
        return new ProviderTable<int?, TBase, TContext>(compiled);
    }
}