using System.Collections.Generic;
using UnityEngine;

namespace Dawn;
public class CurveTableBuilder<TBase, TContext> where TBase : INamespaced<TBase>, ITaggable
{
    private List<IContextualProvider<AnimationCurve?, TBase, TContext>> _baseProviders = [];
    private List<IContextualProvider<AnimationCurve?, TBase, TContext>> _tagProviders = [];
    private IContextualProvider<AnimationCurve?, TBase, TContext>? _global;

    public CurveTableBuilder<TBase, TContext> AddCurve(NamespacedKey<TBase> key, AnimationCurve curve)
    {
        _baseProviders.Add(new MatchingKeyContextualProvider<AnimationCurve, TBase, TContext>(key, curve));
        return this;
    }

    public CurveTableBuilder<TBase, TContext> AddTagCurve(NamespacedKey tag, AnimationCurve curve)
    {
        _tagProviders.Add(new HasTagContextualProvider<AnimationCurve, TBase, TContext>(tag, curve));
        return this;
    }

    public CurveTableBuilder<TBase, TContext> SetGlobalCurve(AnimationCurve curve)
    {
        return SetGlobalCurve(new SimpleContextualProvider<AnimationCurve?, TBase, TContext>(curve));
    }
    public CurveTableBuilder<TBase, TContext> SetGlobalCurve(IContextualProvider<AnimationCurve?, TBase, TContext> provider)
    {
        _global = provider;
        return this;
    }

    public ProviderTable<AnimationCurve?, TBase, TContext> Build()
    {
        List<IContextualProvider<AnimationCurve?, TBase, TContext>> compiled = [.. _baseProviders, .. _tagProviders];
        if (_global != null)
        {
            compiled.Add(_global);
        }
        return new ProviderTable<AnimationCurve?, TBase, TContext>(compiled);
    }
}