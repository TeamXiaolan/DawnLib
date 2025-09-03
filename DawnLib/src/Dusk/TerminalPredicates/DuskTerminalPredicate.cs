using UnityEngine;

namespace Dawn.Dusk;
public abstract class DuskTerminalPredicate : ScriptableObject, ITerminalPurchasePredicate
{
    public abstract void Register(string id);
    public abstract TerminalPurchaseResult CanPurchase();
}