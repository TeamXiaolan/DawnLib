using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dawn.Internal;
using MonoMod.Cil;
using UnityEngine;

namespace Dawn;

static class RoundLoadingStepRegistrationHandler
{
    internal static List<DawnRoundLoadingStepInfo> orderedRoundLoadingSteps = new();
    private static TaskCompletionSource<object?> _dungeonCompletionSource = new();

    internal static void Init()
    {
        RegisterVanillaRoundLoadingSteps();

        LethalContent.Moons.OnFreezeWithContext += (_) => SortRoundLoadingSteps();

        IL.RoundManager.GenerateNewLevelClientRpc += StartInjectInteriorLoadingStep;
        IL.RoundManager.FinishGeneratingNewLevelClientRpc += FinishInjectInteriorLoadingStep;
    }

    static void StartInjectInteriorLoadingStep(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before,
            il => il.MatchLdarg(0),
            il => il.MatchLdcI4(0),
            il => il.MatchStfld<RoundManager>(nameof(RoundManager.dungeonCompletedGenerating))
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match RoundManager.GenerateNewLevelClientRpc (1) IL.");
            return;
        }

        cursor.EmitDelegate(HandleInteriorLoadingStep);
    }

    static void FinishInjectInteriorLoadingStep(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.LoadingScreen)),
            il => il.MatchLdstr("IsLoading"),
            il => il.MatchLdcI4(0),
            il => il.MatchCallvirt<UnityEngine.Animator>(nameof(UnityEngine.Animator.SetBool))
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match RoundManager.FinishGeneratingNewLevelClientRpc (1) IL.");
            return;
        }

        cursor.EmitDelegate(FinishInteriorLoadingStep);
    }

    static void HandleInteriorLoadingStep()
    {
        DawnRoundLoadingStepInfo interiorLoadingStepEntry = LethalContent.RoundLoadingSteps[NamespacedKey<DawnRoundLoadingStepInfo>.Vanilla("interior_loading")];
        interiorLoadingStepEntry.Callback.Invoke(new EnteringAtmosphereLoadingContext());
    }

    static async void FinishInteriorLoadingStep()
    {
        DawnRoundLoadingStepInfo interiorLoadingStepEntry = LethalContent.RoundLoadingSteps[NamespacedKey<DawnRoundLoadingStepInfo>.Vanilla("interior_loading")];
        _dungeonCompletionSource.SetResult(null);
        List<DawnRoundLoadingStepInfo> dependencies = interiorLoadingStepEntry.GetOrderedDependencies();
        foreach (DawnRoundLoadingStepInfo dependency in dependencies)
        {
            await dependency.Callback.Invoke(new EnteringAtmosphereLoadingContext());
        }
    }

    #region Vanilla Round Loading Steps
    private static void RegisterVanillaRoundLoadingSteps()
    {
        DawnLib.DefineRoundLoadingStep(NamespacedKey<DawnRoundLoadingStepInfo>.Vanilla("interior_loading"), InteriorLoading, _ => { }, true);
    }

    static Task InteriorLoading(ILoadingContext context)
    {
        context.SetText($"Random seed: {StartOfRoundRefs.Instance.randomMapSeed}");
        context.SetColor(Color.blue);
        DawnPlugin.Logger.LogFatal("Loading interior...");
        return Task.CompletedTask;
    }
    #endregion

    #region Sorting
    private static void SortRoundLoadingSteps()
    {
        Dictionary<NamespacedKey, DawnRoundLoadingStepInfo> entriesByKey = [];
        foreach (DawnRoundLoadingStepInfo entry in LethalContent.RoundLoadingSteps.Values)
        {
            if (!entriesByKey.TryAdd(entry.Key, entry))
            {
                DawnPlugin.Logger.LogWarning($"Duplicate round loading step key registered: {entry.Key}");
            }
        }

        Dictionary<NamespacedKey, DawnRoundLoadingStepInfo> eligibleByKey = new(entriesByKey);

        bool removedAny;
        do
        {
            removedAny = false;

            foreach (DawnRoundLoadingStepInfo entry in eligibleByKey.Values.ToArray())
            {
                foreach (NamespacedKey dependency in entry.HardDependencies)
                {
                    if (eligibleByKey.ContainsKey(dependency))
                    {
                        continue;
                    }

                    DawnPlugin.Logger.LogError($"Round loading step {entry.Key} was removed because hard dependency {dependency} is missing or unavailable.");

                    eligibleByKey.Remove(entry.Key);
                    removedAny = true;
                    break;
                }
            }
        }
        while (removedAny);

        Dictionary<DawnRoundLoadingStepInfo, HashSet<DawnRoundLoadingStepInfo>> stepToDependencies = [];

        foreach (DawnRoundLoadingStepInfo entry in eligibleByKey.Values)
        {
            HashSet<DawnRoundLoadingStepInfo> dependencies = [];

            foreach (NamespacedKey dependency in entry.HardDependencies)
            {
                if (eligibleByKey.TryGetValue(dependency, out DawnRoundLoadingStepInfo? dependencyEntry))
                {
                    dependencies.Add(dependencyEntry);
                }
            }

            foreach (NamespacedKey dependency in entry.SoftDependencies)
            {
                if (eligibleByKey.TryGetValue(dependency, out DawnRoundLoadingStepInfo? dependencyEntry))
                {
                    dependencies.Add(dependencyEntry);
                }
            }

            stepToDependencies[entry] = dependencies;
        }

        List<DawnRoundLoadingStepInfo> sorted = [];

        while (stepToDependencies.Count > 0)
        {
            List<DawnRoundLoadingStepInfo> ready = stepToDependencies
                .Where(pair => pair.Value.Count == 0)
                .Select(pair => pair.Key)
                .OrderBy(entry => entry.Key.Key)
                .ThenBy(entry => entry.Key.Namespace)
                .ToList();

            if (ready.Count == 0)
            {
                DawnPlugin.Logger.LogError("Circular round loading step dependency detected.");
                LogCircularDependency(stepToDependencies);

                // sorted.AddRange(stepToDependencies.Keys.OrderBy(entry => entry.NamespacedKey.Key).ThenBy(entry => entry.NamespacedKey.Namespace));
                break;
            }

            foreach (DawnRoundLoadingStepInfo entry in ready)
            {
                sorted.Add(entry);
                stepToDependencies.Remove(entry);

                foreach (HashSet<DawnRoundLoadingStepInfo> dependencies in stepToDependencies.Values)
                {
                    dependencies.Remove(entry);
                }
            }
        }

        orderedRoundLoadingSteps.Clear();
        orderedRoundLoadingSteps.AddRange(sorted);

        DawnPlugin.Logger.LogInfo($"Finished sorting {orderedRoundLoadingSteps.Count} round loading steps.");

        foreach (DawnRoundLoadingStepInfo entry in orderedRoundLoadingSteps)
        {
            DawnPlugin.Logger.LogInfo($"Round loading step registered: {entry.Key}");
        }

        LethalContent.RoundLoadingSteps.Freeze();
    }

    private enum VisitState
    {
        Visiting,
        Visited
    }

    private static void LogCircularDependency(Dictionary<DawnRoundLoadingStepInfo, HashSet<DawnRoundLoadingStepInfo>> stepToDependencies)
    {
        List<DawnRoundLoadingStepInfo> cycle = FindDependencyCycle(stepToDependencies);

        if (cycle.Count == 0)
        {
            DawnPlugin.Logger.LogError("Could not resolve exact circular dependency chain. Remaining unresolved steps:");

            foreach ((DawnRoundLoadingStepInfo entry, HashSet<DawnRoundLoadingStepInfo> dependencies) in stepToDependencies)
            {
                string dependencyText = string.Join(", ", dependencies.Select(dependency => dependency.Key));
                DawnPlugin.Logger.LogError($"- {entry.Key} depends on: {dependencyText}");
            }

            return;
        }

        DawnPlugin.Logger.LogError("Circular dependency chain:");

        for (int i = 0; i < cycle.Count - 1; i++)
        {
            DawnRoundLoadingStepInfo entry = cycle[i];
            DawnRoundLoadingStepInfo dependency = cycle[i + 1];

            string dependencyType = GetDependencyType(entry, dependency);

            DawnPlugin.Logger.LogError($"- {entry.Key} {dependencyType} depends on {dependency.Key}");
        }
    }

    private static List<DawnRoundLoadingStepInfo> FindDependencyCycle(Dictionary<DawnRoundLoadingStepInfo, HashSet<DawnRoundLoadingStepInfo>> stepToDependencies)
    {
        Dictionary<DawnRoundLoadingStepInfo, VisitState> states = [];
        List<DawnRoundLoadingStepInfo> stack = [];

        foreach (DawnRoundLoadingStepInfo entry in stepToDependencies.Keys)
        {
            if (states.ContainsKey(entry))
            {
                continue;
            }

            if (TryFindDependencyCycle(entry, stepToDependencies, states, stack, out List<DawnRoundLoadingStepInfo> cycle))
            {
                return cycle;
            }
        }

        return [];
    }

    private static bool TryFindDependencyCycle(DawnRoundLoadingStepInfo entry, Dictionary<DawnRoundLoadingStepInfo, HashSet<DawnRoundLoadingStepInfo>> stepToDependencies, Dictionary<DawnRoundLoadingStepInfo, VisitState> states, List<DawnRoundLoadingStepInfo> stack, out List<DawnRoundLoadingStepInfo> cycle)
    {
        states[entry] = VisitState.Visiting;
        stack.Add(entry);

        foreach (DawnRoundLoadingStepInfo dependency in stepToDependencies[entry])
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

    private static string GetDependencyType(DawnRoundLoadingStepInfo entry, DawnRoundLoadingStepInfo dependency)
    {
        if (entry.HardDependencies.Contains(dependency.Key))
        {
            return "hard";
        }

        if (entry.SoftDependencies.Contains(dependency.Key))
        {
            return "soft";
        }

        return "unknown";
    }
    #endregion
}