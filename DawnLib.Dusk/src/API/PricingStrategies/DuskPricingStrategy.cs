using Dawn;
using UnityEngine;

namespace Dusk;
public abstract class DuskPricingStrategy : ScriptableObject, IProvider<int>
{
    public abstract void Register(NamespacedKey id);

    public abstract int Provide();
}