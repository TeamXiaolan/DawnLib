using System;
using System.Collections.Generic;

namespace CodeRebirthLib;
public class UnlockableInfoBuilder : BaseInfoBuilder<CRUnlockableItemInfo, UnlockableItem, UnlockableInfoBuilder>
{
    public class SuitBuilder
    {
        private UnlockableInfoBuilder _parentBuilder;

        internal SuitBuilder(UnlockableInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal CRSuitInfo Build()
        {
            return new CRSuitInfo();
        }
    }

    public class PlaceableObjectBuilder
    {
        private UnlockableInfoBuilder _parentBuilder;

        internal PlaceableObjectBuilder(UnlockableInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal PlaceableObjectBuilder SetDecor()
        {
            _parentBuilder.value.alwaysInStock = false;
            return this;
        }

        internal PlaceableObjectBuilder SetShipUpgrade()
        {
            _parentBuilder.value.alwaysInStock = true;
            return this;
        }

        internal CRPlaceableObjectInfo Build()
        {
            return new CRPlaceableObjectInfo();
        }
    }

    private int? _cost;
    private CRSuitInfo? _suitInfo;
    private CRPlaceableObjectInfo? _placeableObjectInfo;
    private ITerminalPurchasePredicate? _purchasePredicate;
    
    internal UnlockableInfoBuilder(NamespacedKey<CRUnlockableItemInfo> key, UnlockableItem unlockableItem) : base(key, unlockableItem)
    {
    }

    public UnlockableInfoBuilder SetCost(int cost)
    {
        _cost = cost;
        return this;
    }

    public UnlockableInfoBuilder DefineShop(Action<PlaceableObjectBuilder> callback)
    {
        PlaceableObjectBuilder builder = new(this);
        callback(builder);
        _placeableObjectInfo = builder.Build();
        return this;
    }

    public UnlockableInfoBuilder DefineSuit(Action<SuitBuilder> callback)
    {
        SuitBuilder builder = new(this);
        callback(builder);
        _suitInfo = builder.Build();
        return this;
    }

    public UnlockableInfoBuilder SetPurchasePredicate(ITerminalPurchasePredicate predicate)
    {
        _purchasePredicate = predicate;
        return this;
    }

    override internal CRUnlockableItemInfo Build()
    {
        int cost = 0;
        if (_cost == null && value.shopSelectionNode == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Unlockable: '{key}' didn't set cost. If you intend to have no cost, call .SetCost(0) or provide a ShopSelectionNode with a cost inside to use.");
        }
        else if (_cost == null)
        {
            cost = value.shopSelectionNode.itemCost;
        }

        _purchasePredicate ??= new AlwaysAvaliableTerminalPredicate();
        return new CRUnlockableItemInfo(_purchasePredicate, key, tags, value, cost, _suitInfo, _placeableObjectInfo);
    }
}