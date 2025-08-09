using System;
using UnityEngine.PlayerLoop;

namespace CodeRebirthLib;
public class ItemInfoBuilder
{
    public class ScrapBuilder
    {
        private ItemInfoBuilder _parentBuilder;
        
        internal ScrapBuilder(ItemInfoBuilder parent)
        {
            _parentBuilder = parent;
        }

        public ScrapBuilder AddMoonWeight(NamespacedKey<CRMoonInfo> moon, int weight)
        {
            return this;
        }
        
        internal CRScrapItemInfo Build()
        {
            return new CRScrapItemInfo();
        }
    }

    public class ShopBuilder
    {
        private ItemInfoBuilder _parentBuilder;
        
        private TerminalNode? _infoNode, _requestNode, _receiptNode;
        private int? _costOverride;

        internal ShopBuilder(ItemInfoBuilder parent)
        {
            _parentBuilder = parent;
        }
        
        public ShopBuilder OverrideCost(int cost)
        {
            _costOverride = cost;
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

            if (_infoNode == null)
            {
                _infoNode = new TerminalNodeBuilder($"{_parentBuilder._item.itemName}InfoNode")
                    .SetDisplayText($"[No information about this object was found.]\n\n")
                    .SetClearPreviousText(true)
                    .SetMaxCharactersToType(25)
                    .Build();
            }
            
            return new CRShopItemInfo(_infoNode, _requestNode, _receiptNode, _costOverride ?? _parentBuilder._item.creditsWorth);
        }
    }
    
    private NamespacedKey<CRItemInfo> _key;
    private Item _item;

    private CRScrapItemInfo? _scrapInfo;
    private CRShopItemInfo? _shopInfo;
    
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

    internal CRItemInfo Build()
    {
        return new CRItemInfo(_key, _item, _scrapInfo, _shopInfo);
    }
}