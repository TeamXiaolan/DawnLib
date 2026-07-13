namespace Dawn;

public sealed class DawnWeightedValue<T>
{
    public DawnWeightedValue(WeightChannel<T> channel)
    {
        Channel = channel;
        Profile = new WeightProfile<T>(channel.Policy);
    }

    public DawnWeightedValue(WeightChannel<T> channel, WeightProfile<T> profile)
    {
        Channel = channel;
        Profile = profile;
    }

    public WeightChannel<T> Channel { get; }

    public WeightProfile<T> Profile { get; }

    public T GetValue(WeightQuery query)
    {
        query = query with
        {
            Channel = Channel.Key
        };

        return DawnLib.Weights.Evaluate(Profile, query);
    }

    public WeightModifierHandle AddSource(IWeightModifierSource<T> source)
    {
        return Profile.AddSource(source);
    }

    public bool RemoveSource(WeightModifierHandle handle)
    {
        return Profile.RemoveSource(handle);
    }
}