namespace Dawn;

public class SimpleWeighted(int weight) : IWeighted
{
    public int GetWeight() => weight;
}