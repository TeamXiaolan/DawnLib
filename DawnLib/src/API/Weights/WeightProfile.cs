using System;
using System.Collections.Generic;
using System.Linq;

namespace Dawn;

public sealed class WeightProfile<T> : IWeightProfile
{
    private readonly List<Entry> _sources = new();
    private readonly List<IWeightModifier<T>> _compiled = new();
    private readonly IWeightValuePolicy<T> _policy;

    private bool _dirty = true;

    public WeightProfile(IWeightValuePolicy<T> policy)
    {
        _policy = policy;
        DawnLib.Weights.RegisterProfile(this);
    }

    public WeightModifierHandle AddSource(IWeightModifierSource<T> source)
    {
        WeightModifierHandle handle = WeightModifierHandle.New();

        _sources.Add(new Entry(handle, source));
        MarkDirty();

        return handle;
    }

    public bool RemoveSource(WeightModifierHandle handle)
    {
        int removed = _sources.RemoveAll(x => x.Handle == handle);

        if (removed <= 0)
            return false;

        MarkDirty();
        return true;
    }

    public void MarkDirty()
    {
        _dirty = true;
    }

    public void Rebuild(WeightBuildContext context)
    {
        _compiled.Clear();
        foreach (Entry entry in _sources)
        {
            try
            {
                entry.Source.RefreshSource(context);
                entry.Source.Build(context, _compiled);
            }
            catch (Exception ex)
            {
                DawnPlugin.Logger.LogError($"Failed to build weight source {entry.Source}:\n{ex}");
            }
        }

        _dirty = false;
    }

    public T Evaluate(WeightContext context)
    {
        if (_dirty)
        {
            Rebuild(new WeightBuildContext());
        }

        T value = _policy.InitialValue;
        foreach (IWeightModifier<T> modifier in _compiled.OrderBy(x => x.Phase).ThenBy(x => x.Priority))
        {
            if (!modifier.CanApply(context))
                continue;

            modifier.Apply(ref value, context);
        }

        return _policy.Finalize(value, context);
    }

    public static WeightProfile<T> Create(IWeightValuePolicy<T> policy) => new(policy);
    public static WeightProfile<T> Create(IWeightValuePolicy<T> policy, Action<WeightProfile<T>> callback) => new(policy);

    private readonly record struct Entry(WeightModifierHandle Handle, IWeightModifierSource<T> Source);
}