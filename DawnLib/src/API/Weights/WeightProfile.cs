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

    public Type ValueType => typeof(T);

    public WeightModifierHandle AddSource(IWeightModifierSource<T> source)
    {
        WeightModifierHandle handle = WeightModifierHandle.New();

        _sources.Add(new Entry(handle, source));
        Refresh();

        return handle;
    }

    public bool RemoveSource(WeightModifierHandle handle)
    {
        int removed = _sources.RemoveAll(x => x.Handle == handle);

        if (removed <= 0)
            return false;

        Refresh();
        return true;
    }

    public void ClearSources()
    {
        _sources.Clear();
        Refresh();
    }

    public void Refresh()
    {
        _dirty = true;
        DawnLib.Weights.NotifyProfilesChanged();
    }

    public T Evaluate(WeightContext context)
    {
        RebuildIfDirty();

        T value = _policy.InitialValue;

        foreach (IWeightModifier<T> modifier in _compiled.OrderBy(x => x.Phase).ThenBy(x => x.Priority))
        {
            if (!modifier.CanApply(context))
                continue;

            modifier.Apply(ref value, context);
        }

        return _policy.Finalize(value, context);
    }

    void IWeightProfile.Refresh()
    {
        Refresh();
    }

    private void RebuildIfDirty()
    {
        if (!_dirty)
            return;

        _compiled.Clear();

        WeightBuildContext buildContext = new();

        foreach (Entry entry in _sources)
        {
            try
            {
                entry.Source.Build(buildContext, _compiled);
            }
            catch (Exception ex)
            {
                DawnPlugin.Logger.LogError($"Failed to build weight source {entry.Source}:\n{ex}");
            }
        }

        _dirty = false;
    }

    public static WeightProfile<T> Create(IWeightValuePolicy<T> policy) => new(policy);
    public static WeightProfile<T> Create(IWeightValuePolicy<T> policy, Action<WeightProfile<T>> callback) => new(policy);

    private readonly record struct Entry(WeightModifierHandle Handle, IWeightModifierSource<T> Source);
}