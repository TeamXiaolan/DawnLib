using System;
using System.Linq;
using UnityEngine;

namespace Dawn;
public class UnlockableInfoBuilder : BaseInfoBuilder<DawnUnlockableItemInfo, UnlockableItem, UnlockableInfoBuilder>
{
    public class SuitBuilder
    {
        private UnlockableInfoBuilder _parentBuilder;

        private Material? _suitMaterial;
        private AudioClip? _jumpAudioClip;

        internal SuitBuilder(UnlockableInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        internal SuitBuilder OverrideSuitMaterial(Material suitMaterial)
        {
            _suitMaterial = suitMaterial;
            return this;
        }

        internal SuitBuilder OverrideJumpAudioClip(AudioClip jumpAudioClip)
        {
            _jumpAudioClip = jumpAudioClip;
            return this;
        }

        internal DawnSuitInfo Build()
        {
            if (_suitMaterial == null)
            {
                DawnPlugin.Logger.LogWarning($"Suit: '{_parentBuilder.key}' didn't set suit material.");
            }
            return new DawnSuitInfo(_suitMaterial, _jumpAudioClip);
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
            _parentBuilder.value.unlockableType = 1;
            return this;
        }

        internal PlaceableObjectBuilder SetShipUpgrade()
        {
            _parentBuilder.value.alwaysInStock = true;
            _parentBuilder.value.unlockableType = 1;
            return this;
        }

        internal DawnPlaceableObjectInfo Build()
        {
            return new DawnPlaceableObjectInfo();
        }
    }

    private IProvider<int>? _cost;
    private DawnSuitInfo? _suitInfo;
    private DawnPlaceableObjectInfo? _placeableObjectInfo;
    private ITerminalPurchasePredicate? _purchasePredicate;
    private string _infoNodeText = string.Empty;

    internal UnlockableInfoBuilder(NamespacedKey<DawnUnlockableItemInfo> key, UnlockableItem unlockableItem) : base(key, unlockableItem)
    {
    }

    public UnlockableInfoBuilder SetInfoText(string infoText)
    {
        _infoNodeText = infoText;
        return this;
    }

    public UnlockableInfoBuilder SetCost(int cost)
    {
        return SetCost(new SimpleProvider<int>(cost));
    }

    public UnlockableInfoBuilder SetCost(IProvider<int> cost)
    {
        _cost = cost;
        return this;
    }

    public UnlockableInfoBuilder DefinePlaceableObject(Action<PlaceableObjectBuilder> callback)
    {
        PlaceableObjectBuilder builder = new(this);
        callback(builder);
        _placeableObjectInfo = builder.Build();
        return this;
    }

    public UnlockableInfoBuilder DefineSuit(Action<SuitBuilder> callback)
    {
        value.unlockableType = 0;
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

    override internal DawnUnlockableItemInfo Build()
    {
        if (!value.alreadyUnlocked && !value.shopSelectionNode)
        {
            CompatibleNoun confirmBuyCompatibleNoun = new();
            CompatibleNoun cancelDenyCompatibleNoun = new();

            value.shopSelectionNode = new TerminalNodeBuilder($"{value.unlockableName}ShopSelectionNode")
                .SetDisplayText($"You have requested to order {value.unlockableName}.\nTotal cost of item: [totalCost].\n\nPlease CONFIRM or DENY.\n")
                .SetMaxCharactersToType(15)
                .SetShipUnlockableIndex(-1)
                .SetClearPreviousText(true)
                .SetCreatureName(value.unlockableName)
                .SetOverrideOptions(true)
                .SetTerminalOptions([confirmBuyCompatibleNoun, cancelDenyCompatibleNoun])
                .Build();
        }

        TerminalNode? infoNode = null;
        if (!string.IsNullOrEmpty(_infoNodeText))
        {
            infoNode = new TerminalNodeBuilder($"{value.unlockableName}Info")
                .SetDisplayText(_infoNodeText)
                .SetClearPreviousText(true)
                .SetMaxCharactersToType(35)
                .Build();
        }

        IProvider<int>? cost = _cost;
        if (cost == null && value.shopSelectionNode == null)
        {
            cost = new SimpleProvider<int>(-999); // obvious wrong value
            DawnPlugin.Logger.LogWarning($"Unlockable: '{key}' didn't set cost. If you intend to have no cost, call .SetCost(0) or provide a ShopSelectionNode with a cost inside to use.");
        }
        else if (cost == null)
        {
            cost = new SimpleProvider<int>(value.shopSelectionNode.itemCost);
        }

        _purchasePredicate ??= ITerminalPurchasePredicate.AlwaysSuccess();
        return new DawnUnlockableItemInfo(key, tags, value, new DawnPurchaseInfo(cost, _purchasePredicate), _suitInfo, _placeableObjectInfo, value.shopSelectionNode, value.shopSelectionNode?.terminalOptions?.FirstOrDefault()?.result, null, infoNode, customData);
    }
}