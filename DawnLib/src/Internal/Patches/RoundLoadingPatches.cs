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

        Dictionary<NamespacedKey, RoundLoadingStepEntry> eligibleByKey = new(entriesByKey);

        bool removedAny;
        do
        {
            removedAny = false;

            foreach (RoundLoadingStepEntry entry in eligibleByKey.Values.ToArray())
            {
                foreach (NamespacedKey dependency in entry.HardDependencies)
                {
                    if (eligibleByKey.ContainsKey(dependency))
                    {
                        continue;
                    }

                    DawnPlugin.Logger.LogError($"Round loading step {entry.NamespacedKey} was removed because hard dependency {dependency} is missing or unavailable.");

                    eligibleByKey.Remove(entry.NamespacedKey);
                    removedAny = true;
                    break;
                }
            }
        }
        while (removedAny);

        Dictionary<RoundLoadingStepEntry, HashSet<RoundLoadingStepEntry>> stepToDependencies = [];

        foreach (RoundLoadingStepEntry entry in eligibleByKey.Values)
        {
            HashSet<RoundLoadingStepEntry> dependencies = [];

            foreach (NamespacedKey dependency in entry.HardDependencies)
            {
                if (eligibleByKey.TryGetValue(dependency, out RoundLoadingStepEntry? dependencyEntry))
                {
                    dependencies.Add(dependencyEntry);
                }
            }

            foreach (NamespacedKey dependency in entry.SoftDependencies)
            {
                if (eligibleByKey.TryGetValue(dependency, out RoundLoadingStepEntry? dependencyEntry))
                {
                    dependencies.Add(dependencyEntry);
                }
            }

            stepToDependencies[entry] = dependencies;
        }

        List<RoundLoadingStepEntry> sorted = [];

        while (stepToDependencies.Count > 0)
        {
            List<RoundLoadingStepEntry> ready = stepToDependencies
                .Where(pair => pair.Value.Count == 0)
                .Select(pair => pair.Key)
                .OrderBy(entry => entry.NamespacedKey.Key)
                .ThenBy(entry => entry.NamespacedKey.Namespace)
                .ToList();

            if (ready.Count == 0)
            {
                DawnPlugin.Logger.LogError("Circular round loading step dependency detected.");
                LogCircularDependency(stepToDependencies);

                // sorted.AddRange(stepToDependencies.Keys.OrderBy(entry => entry.NamespacedKey.Key).ThenBy(entry => entry.NamespacedKey.Namespace));
                break;
            }

            foreach (RoundLoadingStepEntry entry in ready)
            {
                sorted.Add(entry);
                stepToDependencies.Remove(entry);

                foreach (HashSet<RoundLoadingStepEntry> dependencies in stepToDependencies.Values)
                {
                    dependencies.Remove(entry);
                }
            }
        }

        _roundLoadingSteps.Clear();
        _roundLoadingSteps.AddRange(sorted);

        DawnPlugin.Logger.LogInfo($"Finished sorting {_roundLoadingSteps.Count} round loading steps.");

        foreach (RoundLoadingStepEntry entry in _roundLoadingSteps)
        {
            DawnPlugin.Logger.LogInfo($"Round loading step registered: {entry.NamespacedKey}");
        }
    }

    private enum VisitState
    {
        Visiting,
        Visited
    }

    private static void LogCircularDependency(Dictionary<RoundLoadingStepEntry, HashSet<RoundLoadingStepEntry>> stepToDependencies)
    {
        List<RoundLoadingStepEntry> cycle = FindDependencyCycle(stepToDependencies);

        if (cycle.Count == 0)
        {
            DawnPlugin.Logger.LogError("Could not resolve exact circular dependency chain. Remaining unresolved steps:");

            foreach ((RoundLoadingStepEntry entry, HashSet<RoundLoadingStepEntry> dependencies) in stepToDependencies)
            {
                string dependencyText = string.Join(", ", dependencies.Select(dependency => dependency.NamespacedKey));
                DawnPlugin.Logger.LogError($"- {entry.NamespacedKey} depends on: {dependencyText}");
            }

            return;
        }

        DawnPlugin.Logger.LogError("Circular dependency chain:");

        for (int i = 0; i < cycle.Count - 1; i++)
        {
            RoundLoadingStepEntry entry = cycle[i];
            RoundLoadingStepEntry dependency = cycle[i + 1];

            string dependencyType = GetDependencyType(entry, dependency);

            DawnPlugin.Logger.LogError($"- {entry.NamespacedKey} {dependencyType} depends on {dependency.NamespacedKey}");
        }
    }

    private static List<RoundLoadingStepEntry> FindDependencyCycle(Dictionary<RoundLoadingStepEntry, HashSet<RoundLoadingStepEntry>> stepToDependencies)
    {
        Dictionary<RoundLoadingStepEntry, VisitState> states = [];
        List<RoundLoadingStepEntry> stack = [];

        foreach (RoundLoadingStepEntry entry in stepToDependencies.Keys)
        {
            if (states.ContainsKey(entry))
            {
                continue;
            }

            if (TryFindDependencyCycle(entry, stepToDependencies, states, stack, out List<RoundLoadingStepEntry> cycle))
            {
                return cycle;
            }
        }

        return [];
    }

    private static bool TryFindDependencyCycle(RoundLoadingStepEntry entry, Dictionary<RoundLoadingStepEntry, HashSet<RoundLoadingStepEntry>> stepToDependencies, Dictionary<RoundLoadingStepEntry, VisitState> states, List<RoundLoadingStepEntry> stack, out List<RoundLoadingStepEntry> cycle)
    {
        states[entry] = VisitState.Visiting;
        stack.Add(entry);

        foreach (RoundLoadingStepEntry dependency in stepToDependencies[entry])
        {
            if (!stepToDependencies.ContainsKey(dependency))
            {
                continue;
            }

            if (!states.TryGetValue(dependency, out VisitState state))
            {
                if (TryFindDependencyCycle(dependency, stepToDependencies, states, stack, out cycle))
                {
                    return true;
                }

                continue;
            }

            if (state != VisitState.Visiting)
            {
                continue;
            }

            int cycleStartIndex = stack.IndexOf(dependency);

            cycle = stack
                .Skip(cycleStartIndex)
                .ToList();

            cycle.Add(dependency);

            return true;
        }

        stack.RemoveAt(stack.Count - 1);
        states[entry] = VisitState.Visited;

        cycle = [];
        return false;
    }

    private static string GetDependencyType(RoundLoadingStepEntry entry, RoundLoadingStepEntry dependency)
    {
        if (entry.HardDependencies.Contains(dependency.NamespacedKey))
        {
            return "hard";
        }

        if (entry.SoftDependencies.Contains(dependency.NamespacedKey))
        {
            return "soft";
        }

        return "unknown";
    }
}