using Dawn;

namespace Dusk.Weights;

public interface ISpawnWeightRule
{
    bool CanApply(in SpawnWeightContext ctx);

    MathOperation GetOperation(in SpawnWeightContext ctx);

    float Apply(float currentWeight, in SpawnWeightContext ctx);
}

public class MoonRule(MoonWeightTransformer transformer) : ISpawnWeightRule
{
    public bool CanApply(in SpawnWeightContext ctx) => ctx.Moon != null;
    public MathOperation GetOperation(in SpawnWeightContext ctx) => transformer.GetOperation(ctx.Moon!);
    public float Apply(float currentWeight, in SpawnWeightContext ctx) => transformer.GetNewWeight(currentWeight, ctx.Moon!);
}

public class InteriorRule(InteriorWeightTransformer transformer) : ISpawnWeightRule
{
    public bool CanApply(in SpawnWeightContext ctx) => ctx.Dungeon != null;
    public MathOperation GetOperation(in SpawnWeightContext ctx) => transformer.GetOperation(ctx.Dungeon!);
    public float Apply(float currentWeight, in SpawnWeightContext ctx) => transformer.GetNewWeight(currentWeight, ctx.Dungeon!);
}

public class WeatherRule(WeatherWeightTransformer transformer) : ISpawnWeightRule
{
    public bool CanApply(in SpawnWeightContext ctx) => ctx.Weather != null;
    public MathOperation GetOperation(in SpawnWeightContext ctx) => transformer.GetOperation(ctx.Weather!);
    public float Apply(float currentWeight, in SpawnWeightContext ctx) => transformer.GetNewWeight(currentWeight, ctx.Weather!);
}

public class ExtraValueRule<T>(NamespacedKey extraKey, WeightTransformer<T> transformer) : ISpawnWeightRule
{
    private readonly NamespacedKey _extraKey = extraKey;
    private readonly WeightTransformer<T> _transformer = transformer;

    public bool CanApply(in SpawnWeightContext ctx)
    {
        return ctx.Extras.TryGet<T>(_extraKey, out _);
    }

    public MathOperation GetOperation(in SpawnWeightContext ctx)
    {
        if (ctx.Extras.TryGet(_extraKey, out T? value))
        {
            return _transformer.GetOperation(value);
        }

        return MathOperation.Additive;
    }

    public float Apply(float currentWeight, in SpawnWeightContext ctx)
    {
        if (ctx.Extras.TryGet(_extraKey, out T? value))
        {
            return _transformer.GetNewWeight(currentWeight, value);
        }

        return currentWeight;
    }
}

public sealed class RoutePriceRule : ExtraValueRule<int>
{
    public RoutePriceRule(RoutePriceWeightTransformer transformer)
        : base(SpawnWeightExtraKeys.RoutingPriceKey, transformer)
    {
    }
}