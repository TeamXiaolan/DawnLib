using System;
using System.Collections.Generic;
using Dawn;
using UnityEngine;

namespace Dusk;
[CreateAssetMenu(menuName = $"{DuskModConstants.TerminalPredicates}/Terminal Predicate Collection", fileName = "New Terminal Predicate Collection", order = DuskModConstants.PredicateOrder)]
public class TerminalPredicateCollection : DuskTerminalPredicate
{
    [SerializeField] private List<DuskTerminalPredicate> _predicates;
    [SerializeField] private LogicOperation _operation = LogicOperation.And;
    
    public override void Register(NamespacedKey namespacedKey)
    {
        foreach (DuskTerminalPredicate predicate in _predicates)
        {
            predicate.Register(namespacedKey);
        }
    }
    public override TerminalPurchaseResult CanPurchase()
    {
        foreach (DuskTerminalPredicate predicate in _predicates)
        {
            TerminalPurchaseResult result = predicate.CanPurchase();
            if (_operation == LogicOperation.And && result is not TerminalPurchaseResult.SuccessPurchaseResult) // AND operation fail
            {
                return result;
            }

            if (_operation == LogicOperation.Or && result is TerminalPurchaseResult.SuccessPurchaseResult) // OR operation succeed
            {
                return result;
            }
        }
        return TerminalPurchaseResult.Success();
    }
}