
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dawn;

public sealed class DawnRoundLoadingStepInfo : DawnBaseInfo<DawnRoundLoadingStepInfo>
{
    internal DawnRoundLoadingStepInfo(NamespacedKey<DawnRoundLoadingStepInfo> key, HashSet<NamespacedKey> tags, Func<ILoadingContext, Task> callback, List<NamespacedKey> hardDependencies, List<NamespacedKey> softDependencies, IDataContainer? customData) : base(key, tags, customData)
    {
        Callback = callback;
        HardDependencies = hardDependencies;
        SoftDependencies = softDependencies;
    }

    public Func<ILoadingContext, Task> Callback { get; }
    public List<NamespacedKey> HardDependencies { get; }
    public List<NamespacedKey> SoftDependencies { get; }

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