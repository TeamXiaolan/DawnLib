namespace CodeRebirthLib.Data;
class RegistrationSettings<T>(T value, IWeighted rarityProvider) : IWeighted
{
    public T Value { get; } = value;
    public IWeighted RarityProvider { get; } = rarityProvider;

    public int GetWeight() => RarityProvider.GetWeight();
}