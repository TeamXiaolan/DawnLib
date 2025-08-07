namespace CodeRebirthLib.Data;
public class SimpleWeightProvider(int weight) : IWeighted
{
    public int GetWeight() => weight;
}