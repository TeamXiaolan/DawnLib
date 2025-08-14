using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib;
public class CurveTableBuilder<TBase> where TBase : INamespaced<TBase>, ITaggable
{
    private List<IProvider<AnimationCurve?, TBase>> _baseProviders = [];
    private List<IProvider<AnimationCurve?, TBase>> _tagProviders = [];
    private IProvider<AnimationCurve?, TBase>? _global;

    public CurveTableBuilder<TBase> AddCurve(NamespacedKey<TBase> key, AnimationCurve curve)
    {
        _baseProviders.Add(new MatchingKeyProvider<AnimationCurve, TBase>(key, curve));
        return this;
    }

    public CurveTableBuilder<TBase> AddTagCurve(NamespacedKey tag, AnimationCurve curve)
    {
        _tagProviders.Add(new HasTagProvider<AnimationCurve, TBase>(tag, curve));
        return this;
    }

    public CurveTableBuilder<TBase> SetGlobalCurve(AnimationCurve curve)
    {
        return SetGlobalCurve(new SimpleProvider<AnimationCurve?, TBase>(curve));
    }
    public CurveTableBuilder<TBase> SetGlobalCurve(IProvider<AnimationCurve?, TBase> provider)
    {
        _global = provider;
        return this;
    }

    public ProviderTable<AnimationCurve?, TBase> Build()
    {
        List<IProvider<AnimationCurve?, TBase>> compiled = [.. _baseProviders, .. _tagProviders];
        if (_global != null)
        {
            compiled.Add(_global);
        }
        return new ProviderTable<AnimationCurve?, TBase>(compiled);
    }
}