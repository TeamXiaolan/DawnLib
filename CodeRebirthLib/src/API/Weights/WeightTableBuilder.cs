using System.Collections.Generic;

namespace CodeRebirthLib;
public class WeightTableBuilder<TBase> where TBase : INamespaced<TBase>, ITaggable
{
    private List<IProvider<int?, TBase>> _baseProviders = [];
    private List<IProvider<int?, TBase>> _tagProviders = [];
    private IProvider<int?, TBase>? _global;
    
    public WeightTableBuilder<TBase> AddWeight(NamespacedKey<TBase> key, int weight)
    {
        return AddWeight(key, new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase> AddWeight(NamespacedKey<TBase> key, IWeighted weight)
    {
        _baseProviders.Add(new MatchingKeyWeightProvider<TBase>(key, weight));
        return this;
    }

    public WeightTableBuilder<TBase> AddTagWeight(NamespacedKey tag, int weight)
    {
        return AddTagWeight(tag, new SimpleWeighted(weight));
    }

    public WeightTableBuilder<TBase> AddTagWeight(NamespacedKey tag, IWeighted weight)
    {
        _tagProviders.Add(new HasTagWeightProvider<TBase>(tag, weight));
        return this;
    }
    
    public WeightTableBuilder<TBase> SetGlobalWeight(int weight)
    {
        return SetGlobalWeight(new SimpleWeighted(weight));
    }
    public WeightTableBuilder<TBase> SetGlobalWeight(IWeighted weight)
    {
        return SetGlobalWeight(new SimpleWeightProvider<TBase>(weight));
    }
    public WeightTableBuilder<TBase> SetGlobalWeight(IProvider<int?, TBase> provider)
    {
        _global = provider;
        return this;
    }

    public Table<int?,TBase> Build()
    {
        List<IProvider<int?, TBase>> compiled = [.._baseProviders, .._tagProviders];
        if (_global != null)
        {
            compiled.Add(_global);
        }
        return new Table<int?,TBase>(compiled);
    }
}