using Dawn.Utils;

namespace Dawn;

public sealed class IntWeightValuePolicy : IWeightValuePolicy<int>
{
    public static readonly IntWeightValuePolicy ClampZero = new(true);
    public static readonly IntWeightValuePolicy Raw = new(false);

    private readonly bool _clampZero;

    private IntWeightValuePolicy(bool clampZero)
    {
        _clampZero = clampZero;
    }

    public int InitialValue => 0;

    public int Finalize(int value, WeightContext context)
    {
        return _clampZero ? value.Clamp0() : value;
    }
}