using System;
using System.Collections.Generic;

namespace Dawn;

public sealed class DawnWeightSystem
{
    private readonly List<IWeightContextContributor> _contextContributors = new();
    private readonly List<IWeightProfile> _profiles = new();

    public event Action? ProfilesChanged;

    public void AddContextContributor(IWeightContextContributor contributor)
    {
        _contextContributors.Add(contributor);
    }

    public void RegisterProfile(IWeightProfile profile)
    {
        if (_profiles.Contains(profile))
            return;

        _profiles.Add(profile);
    }

    public void NotifyProfilesChanged(bool rebuildImmediately = false)
    {
        WeightBuildContext buildContext = new();
        foreach (IWeightProfile profile in _profiles)
        {
            profile.MarkDirty();

            if (rebuildImmediately)
            {
                profile.Rebuild(buildContext);
            }
        }

        ProfilesChanged?.Invoke();
    }

    public T Evaluate<T>(WeightProfile<T> profile, WeightQuery query)
    {
        WeightContext context = CreateContext(query);
        return profile.Evaluate(context);
    }

    public WeightContext CreateContext(WeightQuery query)
    {
        WeightContextBuilder builder = new(query);

        foreach (IWeightContextContributor contributor in _contextContributors)
        {
            contributor.Contribute(builder);
        }

        return builder.Build();
    }
}