namespace Dawn;

public sealed class DungeonIntWeightModifier : IWeightModifier<int>
{
    private readonly ResolvedNamespacedWeight<DawnDungeonInfo> _weight;

    public DungeonIntWeightModifier(ResolvedNamespacedWeight<DawnDungeonInfo> weight)
    {
        _weight = weight;
    }

    public NamespacedKey Key => DawnKeys.DungeonIntWeight;

    public WeightModifierPhase Phase => IntWeightOperations.GetPhase(_weight.Operation);

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (context.Dungeon == null)
            return false;

        if (context.Dungeon.TypedKey == _weight.Key)
            return true;

        return context.Dungeon.HasTag(_weight.Key);
    }

    public void Apply(ref int value, WeightContext context)
    {
        IntWeightOperations.Apply(ref value, _weight.Operation, _weight.Value);
    }
}