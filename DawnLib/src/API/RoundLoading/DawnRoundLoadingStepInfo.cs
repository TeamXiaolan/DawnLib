
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dawn.Utils;

namespace Dawn;

public sealed class DawnRoundLoadingStepInfo : DawnBaseInfo<DawnRoundLoadingStepInfo>
{
    internal DawnRoundLoadingStepInfo(NamespacedKey<DawnRoundLoadingStepInfo> key, HashSet<NamespacedKey> tags, Func<ILoadingContext, Task> callback, IDataContainer? customData) : base(key, tags, customData)
    {
        Callback = callback;

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

        HardDependencies = hardDependencies;
        SoftDependencies = softDependencies;
    }

    public Func<ILoadingContext, Task> Callback { get; }
    public NamespacedKey[] HardDependencies { get; }
    public NamespacedKey[] SoftDependencies { get; }

    // The idea is that the different steps are only created by dawnlib for vanilla, which others would have to inherently rely on, i.e. relying on `DawnKeys.InteriorLoadingStep`
    public List<DawnRoundLoadingStepInfo> GetOrderedDependencies()
    {
        foreach ((int index, DawnRoundLoadingStepInfo entry) in RoundLoadingStepRegistrationHandler.orderedRoundLoadingSteps.WithIndex())
        {
            if (entry.Equals(this))
            {
                List<DawnRoundLoadingStepInfo> nextEntries = new();
                if (RoundLoadingStepRegistrationHandler.orderedRoundLoadingSteps.Count == index + 1)
                {
                    return nextEntries;
                }

                for (int i = index + 1; i < RoundLoadingStepRegistrationHandler.orderedRoundLoadingSteps.Count; i++)
                {
                    DawnRoundLoadingStepInfo nextEntry = RoundLoadingStepRegistrationHandler.orderedRoundLoadingSteps[i];
                    if (nextEntry.Key.Namespace == NamespacedKey.VanillaNamespace)
                    {
                        return nextEntries;
                    }

                    nextEntries.Add(RoundLoadingStepRegistrationHandler.orderedRoundLoadingSteps[i]);
                }

                return nextEntries;
            }
        }

        throw new ArgumentException($"Could not find round loading step entry with key '{Key}'.");
    }
}