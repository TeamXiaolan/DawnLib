using Dawn;

namespace Nightfall;

public static class NightfallWeightChannels
{
    public static readonly WeightChannel<int> EntityReplacementRarity = WeightChannel<int>.From(NightfallKeys.EntityReplacementRarity, IntWeightValuePolicy.ClampZero);
}