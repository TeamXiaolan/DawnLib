using CodeRebirthLib.Data;

namespace CodeRebirthLib.ConfigManagement.Weights;
public class SimpleWeightProvider(int weight) : IWeighted
{
    public int GetWeight() => weight;
}