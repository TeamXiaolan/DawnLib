using System;
using System.Collections.Generic;

namespace CodeRebirthLib;
public class ItemInfoBuilder
{
    public class ScrapBuilder
    {
        private ItemInfoBuilder _parentBuilder;

        private ProviderTable<int?, CRMoonInfo>? _weights;

        internal ScrapBuilder(ItemInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public ScrapBuilder SetWeights(Action<WeightTableBuilder<CRMoonInfo>> callback)
        {
            WeightTableBuilder<CRMoonInfo> builder = new WeightTableBuilder<CRMoonInfo>();
            callback(builder);
            _weights = builder.Build();
            return this;
        }

        internal CRScrapItemInfo Build()
        {
            if (_weights == null)
            {
                CodeRebirthLibPlugin.Logger.LogWarning($"Scrap item '{_parentBuilder._item.itemName}' didn't set weights. If you intend to have no weights (doing something special), call .SetWeights(() => {{}})");
                _weights = ProviderTable<int?, CRMoonInfo>.Empty();
            }
            return new CRScrapItemInfo(_weights);
        }
    }

    public class ShopBuilder
    {
        private ItemInfoBuilder _parentBuilder;

        private TerminalNode? _infoNode, _requestNode, _receiptNode;
        private int? _costOverride;
        private ITerminalPurchasePredicate? _purchasePredicate;

        internal ShopBuilder(ItemInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public ShopBuilder OverrideCost(int cost)
        {
            _costOverride = cost;
            return this;
        }

        public ShopBuilder SetPurchasePredicate(ITerminalPurchasePredicate predicate)
        {
            _purchasePredicate = predicate;
            return this;
        }

        public ShopBuilder OverrideInfoNode(TerminalNode infoNode)
        {
            _infoNode = infoNode;
            return this;
        }

        public ShopBuilder OverrideRequestNode(TerminalNode requestNode)
        {
            _requestNode = requestNode;
            return this;
        }

        public ShopBuilder OverrideReceiptNode(TerminalNode receiptNode)
        {
            _receiptNode = receiptNode;
            return this;
        }

        internal CRShopItemInfo Build()
        {
            if (_receiptNode == null)
            {
                _receiptNode = new TerminalNodeBuilder($"{_parentBuilder._item.itemName}ReceiptNode")
                    .SetDisplayText($"Ordered [variableAmount] {_parentBuilder._item.itemName}. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n")
                    .SetClearPreviousText(true)
                    .SetMaxCharactersToType(15)
                    .Build();
            }

            if (_requestNode == null)
            {
                _requestNode = new TerminalNodeBuilder($"{_parentBuilder._item.itemName}RequestNode")
                    .SetDisplayText($"You have requested to order {_parentBuilder._item.itemName}. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n")
                    .SetClearPreviousText(true)
                    .SetMaxCharactersToType(35)
                    .Build();
            }

            if (_infoNode == null) // this can be null in vanilla, should we really be creating this?
            {
                _infoNode = new TerminalNodeBuilder($"{_parentBuilder._item.itemName}InfoNode")
                    .SetDisplayText($"[No information about this object was found.]\n\n")
                    .SetClearPreviousText(true)
                    .SetMaxCharactersToType(25)
                    .Build();
            }

            _purchasePredicate ??= new AlwaysAvaliableTerminalPredicate();

            return new CRShopItemInfo(_purchasePredicate, _infoNode, _requestNode, _receiptNode, _costOverride ?? _parentBuilder._item.creditsWorth);
        }
    }

    private NamespacedKey<CRItemInfo> _key;
    private Item _item;

    private CRScrapItemInfo? _scrapInfo;
    private CRShopItemInfo? _shopInfo;
    private List<NamespacedKey> _tags = new();
    
    internal ItemInfoBuilder(NamespacedKey<CRItemInfo> key, Item item)
    {
        _key = key;
        _item = item;
    }

    public ItemInfoBuilder DefineShop(Action<ShopBuilder> callback)
    {
        ShopBuilder builder = new(this);
        callback(builder);
        _shopInfo = builder.Build();
        return this;
    }
    public ItemInfoBuilder DefineScrap(Action<ScrapBuilder> callback)
    {
        ScrapBuilder builder = new(this);
        callback(builder);
        _scrapInfo = builder.Build();
        return this;
    }

    public ItemInfoBuilder AddTag(NamespacedKey tag)
    {
        _tags.Add(tag);
        return this;
    }
    
    internal CRItemInfo Build()
    {
        return new CRItemInfo(_key, _tags, _item, _scrapInfo, _shopInfo);
    }
}