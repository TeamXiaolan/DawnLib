using System;
using System.Threading.Tasks;

namespace Dawn;

public class RoundLoadingStepInfoBuilder : BaseInfoBuilder<DawnRoundLoadingStepInfo, Func<ILoadingContext, Task>, RoundLoadingStepInfoBuilder>
{
    internal RoundLoadingStepInfoBuilder(NamespacedKey<DawnRoundLoadingStepInfo> key, Func<ILoadingContext, Task> value) : base(key, value)
    {
    }

    override internal DawnRoundLoadingStepInfo Build()
    {
        return new DawnRoundLoadingStepInfo(key, [], value, customData);
    }
}