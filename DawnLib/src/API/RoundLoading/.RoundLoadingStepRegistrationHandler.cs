using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dawn.Internal;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dawn;

static class RoundLoadingStepRegistrationHandler
{
    internal static List<DawnRoundLoadingStepInfo> orderedRoundLoadingSteps = new();
    private static bool _dungeonCompletionInProgress = true;
    private static bool _sceneLoadingInProgress = true;

    internal static void Init()
    {
        RegisterVanillaRoundLoadingSteps();

        LethalContent.Moons.OnFreezeWithContext += (_) => SortRoundLoadingSteps();

        IL.RoundManager.GenerateNewLevelClientRpc += StartInjectInteriorLoadingStep;
        IL.RoundManager.FinishGeneratingNewLevelClientRpc += FinishInjectInteriorLoadingStep;

        IL.StartOfRound.SceneManager_OnLoad += StartInjectSceneLoadingStep;
        IL.StartOfRound.SceneManager_OnLoadComplete1 += FinishInjectSceneLoadingStep;
    }

    private static void StartInjectSceneLoadingStep(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before,
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.loadingText)),
            il => il.MatchLdcI4(1),
            il => il.MatchCallvirt<UnityEngine.Behaviour>("set_enabled"),
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.loadingText)),
            il => il.MatchLdstr("LOADING WORLD..."),
            il => il.MatchCallvirt<TMPro.TMP_Text>("set_text")))
        {
            DawnPlugin.Logger.LogError($"Couldn't match StartOfRound.SceneManager_OnLoad (1) IL.");
            return;
        }

        cursor.Index++;
        cursor.RemoveRange(7);
        cursor.EmitDelegate(HandleSceneLoadingStep);
    }

    private static void FinishInjectSceneLoadingStep(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After,
            il => il.MatchLdarg(0),
            il => il.MatchLdfld<StartOfRound>(nameof(StartOfRound.shipDoorsEnabled)),
            il => il.MatchBrtrue(out _),
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.loadingText)),
            il => il.MatchLdcI4(1),
            il => il.MatchCallvirt<UnityEngine.Behaviour>("set_enabled"),
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.LoadingScreen)),
            il => il.MatchLdstr("IsLoading"),
            il => il.MatchLdcI4(1),
            il => il.MatchCallvirt<UnityEngine.Animator>(nameof(UnityEngine.Animator.SetBool)),
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.loadingText)),
            il => il.MatchLdstr("Waiting for crew..."),
            il => il.MatchCallvirt<TMPro.TMP_Text>("set_text")))
        {
            DawnPlugin.Logger.LogError($"Couldn't match StartOfRound.SceneManager_OnLoadComplete1 (1) IL.");
            return;
        }

        FieldInfo sceneLoadingInProgressField = AccessTools.Field(typeof(RoundLoadingStepRegistrationHandler), nameof(RoundLoadingStepRegistrationHandler._sceneLoadingInProgress));

        ILLabel continueVanilla = cursor.DefineLabel();

        cursor.Emit(OpCodes.Ldsfld, sceneLoadingInProgressField);
        cursor.Emit(OpCodes.Brfalse_S, continueVanilla);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.Emit(OpCodes.Ldarg_2);
        cursor.Emit(OpCodes.Ldarg_3);
        cursor.EmitDelegate(FinishSceneLoadingStep);
        cursor.Emit(OpCodes.Ret);

        cursor.MarkLabel(continueVanilla);
    }

    private static void HandleSceneLoadingStep(HUDManager hudManager)
    {
        DawnRoundLoadingStepInfo sceneLoadingStepEntry = LethalContent.RoundLoadingSteps[RoundLoadingStepKeys.CurrentLevelSceneLoading];
        sceneLoadingStepEntry.Callback.Invoke(new EnteringAtmosphereLoadingContext());
        _sceneLoadingInProgress = true;
        hudManager.LoadingScreen.SetBool(IsLoadingHash, true);
        hudManager.loadingText.enabled = true;
    }

    private static async void FinishSceneLoadingStep(StartOfRound startOfRound, ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        _playerLoadedIntoScene.SetResult(null);

        DawnRoundLoadingStepInfo sceneLoadingStepEntry = LethalContent.RoundLoadingSteps[RoundLoadingStepKeys.CurrentLevelSceneLoading];
        List<DawnRoundLoadingStepInfo> dependencies = sceneLoadingStepEntry.GetOrderedDependants();
        foreach (DawnRoundLoadingStepInfo dependency in dependencies)
        {
            await dependency.Callback.Invoke(new EnteringAtmosphereLoadingContext());
        }

        _sceneLoadingInProgress = false;
        startOfRound.SceneManager_OnLoadComplete1(clientId, sceneName, loadSceneMode);
    }

    static void StartInjectInteriorLoadingStep(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before,
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.loadingText)),
            il => il.MatchLdstr("Random seed: {0}"),
            il => il.MatchLdarg(1),
            il => il.MatchBox(typeof(System.Int32)),
            il => il.MatchCall<System.String>("Format"),
            il => il.MatchCallvirt<TMPro.TMP_Text>("set_text"),
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.LoadingScreen)),
            il => il.MatchLdstr("IsLoading"),
            il => il.MatchLdcI4(1),
            il => il.MatchCallvirt<UnityEngine.Animator>(nameof(UnityEngine.Animator.SetBool))
            ))
        {
            DawnPlugin.Logger.LogError($"Couldn't match RoundManager.GenerateNewLevelClientRpc (1) IL.");
            return;
        }

        cursor.Index++;
        cursor.RemoveRange(11);
        cursor.EmitDelegate(HandleInteriorLoadingStep);
    }

    static void FinishInjectInteriorLoadingStep(ILContext il)
    {
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.Before,
            il => il.MatchCall<HUDManager>("get_Instance"),
            il => il.MatchLdfld<HUDManager>(nameof(HUDManager.loadingText)),
            il => il.MatchLdcI4(0),
            il => il.MatchCallvirt<UnityEngine.Behaviour>("set_enabled"),
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

        FieldInfo dungeonCompletionInProgressField = AccessTools.Field(typeof(RoundLoadingStepRegistrationHandler), nameof(RoundLoadingStepRegistrationHandler._dungeonCompletionInProgress));

        ILLabel continueVanilla = cursor.DefineLabel();

        cursor.Emit(OpCodes.Ldsfld, dungeonCompletionInProgressField);
        cursor.Emit(OpCodes.Brfalse_S, continueVanilla);

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(FinishInteriorLoadingStep);
        cursor.Emit(OpCodes.Ret);

        cursor.MarkLabel(continueVanilla);
    }

    private static readonly int IsLoadingHash = Animator.StringToHash("IsLoading"); // Bool
    static void HandleInteriorLoadingStep(HUDManager hudManager)
    {
        DawnRoundLoadingStepInfo interiorLoadingStepEntry = LethalContent.RoundLoadingSteps[RoundLoadingStepKeys.InteriorLoading];
        interiorLoadingStepEntry.Callback.Invoke(new EnteringAtmosphereLoadingContext());
        _dungeonCompletionInProgress = true;
        hudManager.LoadingScreen.SetBool(IsLoadingHash, true);
    }

    private static async void FinishInteriorLoadingStep(RoundManager roundManager)
    {
        DawnRoundLoadingStepInfo interiorLoadingStepEntry = LethalContent.RoundLoadingSteps[RoundLoadingStepKeys.InteriorLoading];
        List<DawnRoundLoadingStepInfo> dependencies = interiorLoadingStepEntry.GetOrderedDependants();
        foreach (DawnRoundLoadingStepInfo dependency in dependencies)
        {
            await dependency.Callback.Invoke(new EnteringAtmosphereLoadingContext());
        }

        _dungeonCompletionInProgress = false;
        roundManager.FinishGeneratingNewLevelClientRpc();
    }

    #region Vanilla Loading Steps
    private static void RegisterVanillaRoundLoadingSteps()
    {
        DawnLib.DefineRoundLoadingStep(RoundLoadingStepKeys.InteriorLoading, InteriorLoading, _ => { }, true);
        DawnLib.DefineRoundLoadingStep(RoundLoadingStepKeys.CurrentLevelSceneLoading, CurrentLevelSceneLoading, _ => { }, true);
        // DawnLib.DefineRoundLoadingStep(NamespacedKey<DawnRoundLoadingStepInfo>.Vanilla("interior"), InteriorLoading, _ => { }, true);
    }

    private static TaskCompletionSource<object?> _playerLoadedIntoScene = new();
    static async Task CurrentLevelSceneLoading(ILoadingContext context)
    {
        ColorUtility.TryParseHtmlString("#3D4A5B", out Color mainTextStartColor);
        ColorUtility.TryParseHtmlString("#7DB5BE", out Color mainTextEndColor);
        ColorUtility.TryParseHtmlString("#465A6F8D", out Color secondaryTextColor);
        ColorUtility.TryParseHtmlString("#0F171F9F", out Color backgroundColor);

        context.SetMainText("ENTERING THE ATMOSPHERE...");
        context.SetMainTextColor(mainTextStartColor, mainTextEndColor);
        context.SetSecondaryText($"LOADING WORLD...");
        context.SetSecondaryTextColor(secondaryTextColor);
        context.SetBackgroundColor(backgroundColor);

        _playerLoadedIntoScene = new();
        await _playerLoadedIntoScene.Task;

        context.SetMainText("ENTERING THE ATMOSPHERE...");
        context.SetMainTextColor(mainTextStartColor, mainTextEndColor);
        context.SetSecondaryText($"Waiting for crew...");
        context.SetSecondaryTextColor(secondaryTextColor);
        context.SetBackgroundColor(backgroundColor);
    }

    static Task InteriorLoading(ILoadingContext context)
    {
        ColorUtility.TryParseHtmlString("#3D4A5B", out Color mainTextStartColor);
        ColorUtility.TryParseHtmlString("#7DB5BE", out Color mainTextEndColor);
        ColorUtility.TryParseHtmlString("#465A6F8D", out Color secondaryTextColor);
        ColorUtility.TryParseHtmlString("#0F171F9F", out Color backgroundColor);

        context.SetMainText("ENTERING THE ATMOSPHERE...");
        context.SetMainTextColor(mainTextStartColor, mainTextEndColor);
        context.SetSecondaryText($"Random seed: {StartOfRoundRefs.Instance.randomMapSeed}");
        context.SetSecondaryTextColor(secondaryTextColor);
        context.SetBackgroundColor(backgroundColor);
        return Task.CompletedTask;
    }
    #endregion

    #region Sorting
    private static void SortRoundLoadingSteps()
    {
        Dictionary<NamespacedKey, DawnRoundLoadingStepInfo> eligibleByKey = [];

        foreach (DawnRoundLoadingStepInfo entry in LethalContent.RoundLoadingSteps.Values)
        {
            if (!eligibleByKey.TryAdd(entry.Key, entry))
            {
                DawnPlugin.Logger.LogWarning($"Duplicate round loading step key registered: {entry.Key}");
            }
        }

        bool removedAny;
        do
        {
            removedAny = false;

            foreach (DawnRoundLoadingStepInfo entry in eligibleByKey.Values.ToArray())
            {
                NamespacedKey? missingDependency = entry.HardDependencies.FirstOrDefault(dependency => !eligibleByKey.ContainsKey(dependency));
                if (missingDependency == null)
                {
                    continue;
                }

                DawnPlugin.Logger.LogError($"Round loading step {entry.Key} was removed because hard dependency {missingDependency} is missing or unavailable.");

                eligibleByKey.Remove(entry.Key);
                removedAny = true;
            }
        }
        while (removedAny);

        orderedRoundLoadingSteps.Clear();
        orderedRoundLoadingSteps.AddRange(SortSteps(eligibleByKey.Values.ToArray()));

        DawnPlugin.Logger.LogInfo($"Finished sorting {orderedRoundLoadingSteps.Count} round loading steps.");

        foreach (DawnRoundLoadingStepInfo entry in orderedRoundLoadingSteps)
        {
            DawnPlugin.Logger.LogInfo($"Round loading step registered: {entry.Key}");
        }

        LethalContent.RoundLoadingSteps.Freeze();
    }

    private static List<DawnRoundLoadingStepInfo> SortSteps(IReadOnlyCollection<DawnRoundLoadingStepInfo> entries)
    {
        Dictionary<NamespacedKey, DawnRoundLoadingStepInfo> entriesByKey = entries.ToDictionary(entry => entry.Key);

        Dictionary<DawnRoundLoadingStepInfo, int> remainingDependencies = [];
        Dictionary<DawnRoundLoadingStepInfo, List<DawnRoundLoadingStepInfo>> dependants = [];

        foreach (DawnRoundLoadingStepInfo entry in entries)
        {
            remainingDependencies[entry] = 0;
            dependants[entry] = [];
        }

        foreach (DawnRoundLoadingStepInfo entry in entries)
        {
            foreach (NamespacedKey dependencyKey in entry.HardDependencies.Concat(entry.SoftDependencies))
            {
                if (!entriesByKey.TryGetValue(dependencyKey, out DawnRoundLoadingStepInfo? dependency))
                {
                    continue;
                }

                remainingDependencies[entry]++;
                dependants[dependency].Add(entry);
            }
        }

        Stack<DawnRoundLoadingStepInfo> ready = new(
            entries.Where(entry => remainingDependencies[entry] == 0).Reverse()
        );

        List<DawnRoundLoadingStepInfo> sorted = [];

        while (ready.TryPop(out DawnRoundLoadingStepInfo? entry))
        {
            sorted.Add(entry);

            // Reverse so registration order is preserved when pushing onto the stack.
            foreach (DawnRoundLoadingStepInfo dependant in dependants[entry].AsEnumerable().Reverse())
            {
                remainingDependencies[dependant]--;

                if (remainingDependencies[dependant] == 0)
                {
                    ready.Push(dependant);
                }
            }
        }

        if (sorted.Count != entries.Count)
        {
            Dictionary<DawnRoundLoadingStepInfo, HashSet<DawnRoundLoadingStepInfo>> unresolved = [];

            foreach (DawnRoundLoadingStepInfo entry in entries)
            {
                if (remainingDependencies[entry] == 0)
                {
                    continue;
                }

                unresolved[entry] = entry
                    .HardDependencies
                    .Concat(entry.SoftDependencies)
                    .Select(key => entriesByKey.GetValueOrDefault(key))
                    .Where(dependency =>
                        dependency != null &&
                        remainingDependencies[dependency] > 0)
                    .ToHashSet()!;
            }

            LogCircularDependency(unresolved);
        }

        return sorted;
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