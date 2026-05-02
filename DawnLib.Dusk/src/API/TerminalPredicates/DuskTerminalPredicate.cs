using Dawn;
using UnityEngine;

namespace Dusk;

[HelpURL("https://thunderstore.io/c/lethal-company/p/TeamXiaolan/DawnLib/wiki/")]
public abstract class DuskTerminalPredicate : ScriptableObject, ITerminalPurchasePredicate
{
    public abstract void Register(NamespacedKey namespacedKey);
    public abstract TerminalPurchaseResult CanPurchase();
}