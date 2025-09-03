using System.Collections.Generic;
using UnityEngine;

namespace CodeRebirthLib;
public class CurveTableBuilder<TBase> where TBase : INamespaced<TBase>, ITaggable
{
    private List<IContextualProvider<AnimationCurve?, TBase>> _baseProviders = [];
    private List<IContextualProvider<AnimationCurve?, TBase>> _tagProviders = [];
    private IContextualProvider<AnimationCurve?, TBase>? _global;

    public CurveTableBuilder<TBase> AddCurve(NamespacedKey<TBase> key, AnimationCurve curve)
    {
        _baseProviders.Add(new MatchingKeyContextualProvider<AnimationCurve, TBase>(key, curve));
        return this;
    }

    public CurveTableBuilder<TBase> AddTagCurve(NamespacedKey tag, AnimationCurve curve)
    {
        _tagProviders.Add(new HasTagContextualProvider<AnimationCurve, TBase>(tag, curve));
        return this;
    }

    public CurveTableBuilder<TBase> SetGlobalCurve(AnimationCurve curve)
    {
        return SetGlobalCurve(new SimpleContextualProvider<AnimationCurve?, TBase>(curve));
    }
    public CurveTableBuilder<TBase> SetGlobalCurve(IContextualProvider<AnimationCurve?, TBase> provider)
    {
        _global = provider;
        return this;
    }

    public ProviderTable<AnimationCurve?, TBase> Build()
    {
        List<IContextualProvider<AnimationCurve?, TBase>> compiled = [.. _baseProviders, .. _tagProviders];
        if (_global != null)
        {
            compiled.Add(_global);
        }
        return new ProviderTable<AnimationCurve?, TBase>(compiled);
    }
}