using Dawn;
using UnityEngine;

namespace Dusk;
public abstract class DuskPredicate : ScriptableObject, IPredicate
{
    public abstract void Register(NamespacedKey namespacedKey);

    public abstract bool Evaluate();
}