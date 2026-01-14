using Dawn;
using UnityEngine;

namespace Dusk;
public abstract class DuskTerminalPredicate : ScriptableObject, ITerminalPurchasePredicate
{
    public abstract void Register(NamespacedKey namespacedKey);
    public abstract TerminalPurchaseResult CanPurchase();
}