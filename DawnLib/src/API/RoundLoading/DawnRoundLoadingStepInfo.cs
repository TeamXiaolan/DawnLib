
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
        HardDependencies = method
            .GetCustomAttributes<LoadingStepHardDependencyAttribute>()
            .Select(attribute => attribute.Dependency)
            .ToArray();

        SoftDependencies = method
            .GetCustomAttributes<LoadingStepSoftDependencyAttribute>()
            .Select(attribute => attribute.Dependency)
            .ToArray();
    }

    public Func<ILoadingContext, Task> Callback { get; }
    public NamespacedKey[] HardDependencies { get; }
    public NamespacedKey[] SoftDependencies { get; }

    public List<DawnRoundLoadingStepInfo> GetOrderedDependants()
    {
        List<DawnRoundLoadingStepInfo> dependants = [];
        HashSet<NamespacedKey> reachableKeys = [Key];

        foreach (DawnRoundLoadingStepInfo entry in RoundLoadingStepRegistrationHandler.orderedRoundLoadingSteps)
        {
            bool dependsOnReachableStep = entry.HardDependencies
                .Concat(entry.SoftDependencies)
                .Any(reachableKeys.Contains);

            if (!dependsOnReachableStep)
            {
                continue;
            }

            dependants.Add(entry);
            reachableKeys.Add(entry.Key);
        }

        return dependants;
    }
}