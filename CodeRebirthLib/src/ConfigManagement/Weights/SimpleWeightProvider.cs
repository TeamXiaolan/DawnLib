namespace CodeRebirthLib.ConfigManagement.Weights;
public class SimpleWeightProvider(int weight) : IWeightProvider
{
    public int GetWeight() => weight;
}