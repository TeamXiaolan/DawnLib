using System.Linq;

namespace Dawn;

public sealed class MoonSceneIntWeightModifier : IWeightModifier<int>
{
    private readonly ResolvedNamespacedWeight<IMoonSceneInfo> _weight;

    public MoonSceneIntWeightModifier(ResolvedNamespacedWeight<IMoonSceneInfo> weight)
    {
        _weight = weight;
    }

    public NamespacedKey Key => DawnKeys.MoonSceneIntWeight;

    public WeightModifierPhase Phase => IntWeightOperations.GetPhase(_weight.Operation);

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (context.Moon == null)
            return false;

        if (!context.TryGet(DawnWeightContextKeys.MoonScene, out IMoonSceneInfo? moonSceneInfo))
            return false;

        if (moonSceneInfo.SceneName != context.Moon.Level.sceneName)
            return false;

        return true;
    }

    public void Apply(ref int value, WeightContext context)
    {
        IntWeightOperations.Apply(ref value, _weight.Operation, _weight.Value);
    }
}