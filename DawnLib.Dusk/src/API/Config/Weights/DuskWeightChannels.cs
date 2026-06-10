using Dawn;

namespace Dusk.Weights;

public static class DuskWeightChannels
{
    public static readonly WeightChannel<int> EntityReplacementRarity =
        WeightChannel<int>.From(DuskKeys.EntityReplacementRarity, IntWeightValuePolicy.ClampZero);
}