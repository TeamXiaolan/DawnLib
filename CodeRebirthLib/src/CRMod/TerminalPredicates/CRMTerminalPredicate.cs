using UnityEngine;

namespace CodeRebirthLib.CRMod;
public abstract class CRMTerminalPredicate : ScriptableObject, ITerminalPurchasePredicate
{
    public abstract void Register(string id);
    public abstract TerminalPurchaseResult CanPurchase();
}