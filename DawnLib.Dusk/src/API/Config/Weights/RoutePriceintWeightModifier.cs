using Dawn;

namespace Dusk.Weights;

public sealed class RoutePriceIntWeightModifier : IWeightModifier<int>
{
    private readonly IntComparisonConfigWeight _config;

    public RoutePriceIntWeightModifier(IntComparisonConfigWeight config)
    {
        _config = config;
    }

    public NamespacedKey Key => DuskKeys.RoutePriceIntWeight;

    public WeightModifierPhase Phase => IntWeightOperations.GetPhase(_config.Operation);

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        if (!context.TryGet(DuskWeightContextKeys.RoutePrice, out int routePrice))
            return false;

        return Matches(routePrice, _config.IntComparison);
    }

    public void Apply(ref int value, WeightContext context)
    {
        IntWeightOperations.Apply(ref value, _config.Operation, _config.Value);
    }

    private static bool Matches(int value, IntComparison comparison)
    {
        return comparison.ComparisonOperation switch
        {
            ComparisonOperation.Equal => value == comparison.Value,
            ComparisonOperation.NotEqual => value != comparison.Value,
            ComparisonOperation.Greater => value > comparison.Value,
            ComparisonOperation.Less => value < comparison.Value,
            ComparisonOperation.GreaterOrEqual => value >= comparison.Value,
            ComparisonOperation.LessOrEqual => value <= comparison.Value,
            _ => false
        };
    }
}