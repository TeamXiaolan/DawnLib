using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dawn.Utils;

namespace Dawn.Internal;

static class RoundLoadingPatches
{
    private static List<RoundLoadingStepEntry> _roundLoadingSteps = new();

    internal static void Init()
    {
        LethalContent.Moons.OnFreezeWithContext += (_) => SortRoundLoadingSteps();
    }

    internal static void AddRoundLoadingStep(NamespacedKey key, Func<IRoundLoadingContext, Task> callback)
    {
        MethodInfo method = callback.Method;

        LoadingStepHardDependencyAttribute[] hardDependencyAttributes = method.GetCustomAttributes<LoadingStepHardDependencyAttribute>().ToArray();
        LoadingStepSoftDependencyAttribute[] softDependencyAttributes = method.GetCustomAttributes<LoadingStepSoftDependencyAttribute>().ToArray();

        NamespacedKey[] hardDependencies = new NamespacedKey[hardDependencyAttributes.Length];
        foreach ((int index, LoadingStepHardDependencyAttribute attribute) in hardDependencyAttributes.WithIndex())
        {
            hardDependencies[index] = attribute.Dependency;
        }

        NamespacedKey[] softDependencies = new NamespacedKey[softDependencyAttributes.Length];
        foreach ((int index, LoadingStepSoftDependencyAttribute attribute) in softDependencyAttributes.WithIndex())
        {
            softDependencies[index] = attribute.Dependency;
        }

        _roundLoadingSteps.Add(new RoundLoadingStepEntry(
            key,
            callback,
            hardDependencies,
            softDependencies
        ));
    }

    private static void SortRoundLoadingSteps()
    {
        if (_roundLoadingSteps.Count == 0)
        {
            return;
        }

        Dictionary<NamespacedKey, RoundLoadingStepEntry> entriesByKey = [];
        foreach (RoundLoadingStepEntry entry in _roundLoadingSteps)
        {
            if (!entriesByKey.TryAdd(entry.NamespacedKey, entry))
            {
                DawnPlugin.Logger.LogWarning($"Duplicate round loading step key registered: {entry.NamespacedKey}");
            }
        }

        List<RoundLoadingStepEntry> eligible = [];
        foreach (RoundLoadingStepEntry entry in _roundLoadingSteps)
        {
            bool add = true;
            foreach (NamespacedKey dependency in entry.HardDependencies)
            {
                bool containsHardDependency = false;
                if (entriesByKey.TryGetValue(dependency, out RoundLoadingStepEntry? dependencyEntry))
                {
                    containsHardDependency = true;
                }

                if (!containsHardDependency)
                {
                    add = false;
                    break;
                }
            }

            if (!add)
            {
                continue;
            }

            eligible.Add(entry);
        }

        Dictionary<RoundLoadingStepEntry, List<RoundLoadingStepEntry>> stepToDependencies = [];
        foreach (RoundLoadingStepEntry entry in eligible)
        {
            stepToDependencies[entry] = new List<RoundLoadingStepEntry>();
            foreach (NamespacedKey dependency in entry.HardDependencies)
            {
                if (entriesByKey.TryGetValue(dependency, out RoundLoadingStepEntry? dependencyEntry))
                {
                    stepToDependencies[entry].Add(dependencyEntry);
                }
            }

            foreach (NamespacedKey dependency in entry.SoftDependencies)
            {
                if (entriesByKey.TryGetValue(dependency, out RoundLoadingStepEntry? dependencyEntry))
                {
                    stepToDependencies[entry].Add(dependencyEntry);
                }
            }
        }

        List<RoundLoadingStepEntry> sorted = [];
        HandleAddingStepToSorted(sorted, stepToDependencies);

        sorted.Sort((a, b) => a.NamespacedKey.Key.CompareTo(b.NamespacedKey.Key));

        while (stepToDependencies.Count > 0)
        {
            HandleAddingStepToSorted(sorted, stepToDependencies);
        }

        DawnPlugin.Logger.LogInfo($"Finished sorting {_roundLoadingSteps.Count} round loading steps.");
        foreach (RoundLoadingStepEntry entry in sorted)
        {
            DawnPlugin.Logger.LogInfo($"Round loading step registered: {entry.NamespacedKey}");
        }

        _roundLoadingSteps.Clear();
        _roundLoadingSteps.AddRange(sorted);
    }

    private static void HandleAddingStepToSorted(List<RoundLoadingStepEntry> sorted, Dictionary<RoundLoadingStepEntry, List<RoundLoadingStepEntry>> stepToDependencies)
    {
        for (int i = stepToDependencies.Count - 1; i >= 0; i--)
        {
            (RoundLoadingStepEntry entry, List<RoundLoadingStepEntry> dependencies) = stepToDependencies.ElementAt(i);
            if (dependencies.Count == 0)
            {
                sorted.Add(entry);
                foreach (RoundLoadingStepEntry dependency in stepToDependencies[entry])
                {
                    stepToDependencies[dependency].Remove(entry);
                }

                stepToDependencies.Remove(entry);
            }
        }
    }
}