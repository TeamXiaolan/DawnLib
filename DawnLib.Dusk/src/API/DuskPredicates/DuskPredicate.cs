using Dawn;
using UnityEngine;

namespace Dusk;

[HelpURL("https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/wiki/4101-c-moons/")]
public abstract class DuskPredicate : ScriptableObject, IPredicate
{
    public abstract void Register(NamespacedKey namespacedKey);

    public abstract bool Evaluate();
}