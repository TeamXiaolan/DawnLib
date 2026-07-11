using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dawn;

public class RoundLoadingStepInfoBuilder : BaseInfoBuilder<DawnRoundLoadingStepInfo, Func<ILoadingContext, Task>, RoundLoadingStepInfoBuilder>
{
    private List<NamespacedKey> _hardDependencies, _softDependencies = new();
    internal RoundLoadingStepInfoBuilder(NamespacedKey<DawnRoundLoadingStepInfo> key, Func<ILoadingContext, Task> value) : base(key, value)
    {
    }

    public RoundLoadingStepInfoBuilder AddHardDependency(NamespacedKey key)
    {
        _hardDependencies.Add(key);
        return this;
    }

    public RoundLoadingStepInfoBuilder AddSoftDependency(NamespacedKey key)
    {
        _softDependencies.Add(key);
        return this;
    }

    override internal DawnRoundLoadingStepInfo Build()
    {
        return new DawnRoundLoadingStepInfo(key, [], value, _hardDependencies, _softDependencies, customData);
    }
}