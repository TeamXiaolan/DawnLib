using Dawn;
using UnityEngine;

namespace Dusk;

public sealed class MapObjectSpawnMechanicsModifier : IWeightModifier<AnimationCurve?>
{
    private readonly MapObjectSpawnMechanics _mechanics;

    public MapObjectSpawnMechanicsModifier(MapObjectSpawnMechanics mechanics)
    {
        _mechanics = mechanics;
    }

    public NamespacedKey Key => DuskKeys.MapObjectSpawnMechanics;

    public WeightModifierPhase Phase => WeightModifierPhase.Override;

    public int Priority => 0;

    public bool CanApply(WeightContext context)
    {
        return true;
    }

    public void Apply(ref AnimationCurve? value, WeightContext context)
    {
        value = _mechanics.GetCurve(context);
    }
}