using Dawn;
using Dusk.Weights.Transformers;

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
