using CodeRebirthLib.ConfigManagement.Weights;
using CodeRebirthLib.Data;

namespace CodeRebirthLib.Patches;
class InjectionSettings<T>(T value, IWeighted rarityProvider) : IWeighted
{
    public T Value { get; } = value;
    public IWeighted RarityProvider { get; } = rarityProvider;

    public int GetWeight() => RarityProvider.GetWeight();
}