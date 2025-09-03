using System.Collections.Generic;

namespace CodeRebirthLib;
public class WeightTableBuilder<TBase> where TBase : INamespaced<TBase>, ITaggable
{
    private List<IContextualProvider<int?, TBase>> _baseProviders = [];
    private List<IContextualProvider<int?, TBase>> _tagProviders = [];
    private IContextualProvider<int?, TBase>? _global;

    public WeightTableBuilder<TBase> AddWeight(NamespacedKey<TBase> key, int weight)
    {
        return AddWeight(key, new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase> AddWeight(NamespacedKey<TBase> key, IWeighted weight)
    {
        _baseProviders.Add(new MatchingKeyWeightContextualProvider<TBase>(key, weight));
        return this;
    }

    public WeightTableBuilder<TBase> AddTagWeight(NamespacedKey tag, int weight)
    {
        return AddTagWeight(tag, new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase> AddTagWeight(NamespacedKey tag, IWeighted weight)
    {
        _tagProviders.Add(new HasTagWeightContextualProvider<TBase>(tag, weight));
        return this;
    }

    public WeightTableBuilder<TBase> SetGlobalWeight(int weight)
    {
        return SetGlobalWeight(new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase> SetGlobalWeight(IWeighted weight)
    {
        return SetGlobalWeight(new SimpleWeightContextualProvider<TBase>(weight));
    }

    public WeightTableBuilder<TBase> SetGlobalWeight(IContextualProvider<int?, TBase> provider)
    {
        _global = provider;
        return this;
    }

    public ProviderTable<int?, TBase> Build()
    {
        List<IContextualProvider<int?, TBase>> compiled = [.. _baseProviders, .. _tagProviders];
        if (_global != null)
        {
            compiled.Add(_global);
        }
        return new ProviderTable<int?, TBase>(compiled);
    }
}