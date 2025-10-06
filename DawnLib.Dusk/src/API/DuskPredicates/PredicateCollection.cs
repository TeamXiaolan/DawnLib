using System.Collections.Generic;
using Dawn;
using UnityEngine;

namespace Dusk;

[CreateAssetMenu(menuName = $"{DuskModConstants.DuskPredicates}/Predicate Collection", fileName = "New Predicate Collections", order = DuskModConstants.PredicateOrder)]
public class PredicateCollection : DuskPredicate
{
    [SerializeField] private List<DuskPredicate> _predicates;
    [SerializeField] private LogicOperation _operation = LogicOperation.And;

    public override void Register(NamespacedKey namespacedKey)
    {
        foreach (DuskPredicate predicate in _predicates)
        {
            predicate.Register(namespacedKey);
        }
    }
    public override bool Evaluate()
    {
        foreach (DuskPredicate predicate in _predicates)
        {
            bool result = predicate.Evaluate();
            if (_operation == LogicOperation.And && !result) // AND operation fail
            {
                return false;
            }
            if (_operation == LogicOperation.Or && result) // OR operation succeed
            {
                return true;
            }
        }
        return false;
    }
}