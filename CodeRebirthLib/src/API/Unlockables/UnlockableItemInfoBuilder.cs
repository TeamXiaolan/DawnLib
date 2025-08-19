using System;
using System.Collections.Generic;

namespace CodeRebirthLib;
public class UnlockableInfoBuilder
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
            _parentBuilder._unlockableItem.alwaysInStock = false;
            return this;
        }

        internal PlaceableObjectBuilder SetShipUpgrade()
        {
            _parentBuilder._unlockableItem.alwaysInStock = true;
            return this;
        }

        internal CRPlaceableObjectInfo Build()
        {
            return new CRPlaceableObjectInfo();
        }
    }

    private NamespacedKey<CRUnlockableItemInfo> _key;
    private UnlockableItem _unlockableItem;

    private int? _cost;
    private CRSuitInfo? _suitInfo;
    private CRPlaceableObjectInfo? _placeableObjectInfo;
    private ITerminalPurchasePredicate? _purchasePredicate;

    private List<NamespacedKey> _tags;
    
    internal UnlockableInfoBuilder(NamespacedKey<CRUnlockableItemInfo> key, UnlockableItem unlockableItem)
    {
        _key = key;
        _unlockableItem = unlockableItem;
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

    public UnlockableInfoBuilder AddTag(NamespacedKey tag)
    {
        _tags.Add(tag);
        return this;
    }
    
    internal CRUnlockableItemInfo Build()
    {
        int cost = 0;
        if (_cost == null && _unlockableItem.shopSelectionNode == null)
        {
            CodeRebirthLibPlugin.Logger.LogWarning($"Unlockable: '{_key}' didn't set cost. If you intend to have no cost, call .SetCost(0) or provide a ShopSelectionNode with a cost inside to use.");
        }
        else if (_cost == null)
        {
            cost = _unlockableItem.shopSelectionNode.itemCost;
        }

        _purchasePredicate ??= new AlwaysAvaliableTerminalPredicate();
        return new CRUnlockableItemInfo(_purchasePredicate, _key, _tags, _unlockableItem, cost, _suitInfo, _placeableObjectInfo);
    }
}