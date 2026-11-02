using Dawn;
using UnityEngine;

namespace Dusk;

[HelpURL("https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/wiki/")]
public abstract class DuskPricingStrategy : ScriptableObject, IProvider<int>
{
    public abstract void Register(NamespacedKey id);

    public abstract int Provide();
}