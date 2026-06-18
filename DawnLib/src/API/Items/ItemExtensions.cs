using System;
using Dawn.Interfaces;

namespace Dawn;

public static class ItemExtensions
{
    extension(Item item)
    {
        public DawnItemInfo DawnInfo
        {
            get => item.GetDawnInfoCore();
            set => item.SetDawnInfoCore(value);
        }

        [Obsolete("Use Item.DawnInfo instead")]
        public DawnItemInfo GetDawnInfo()
        {
            return item.GetDawnInfoCore();
        }

        [Obsolete("Use Item.DawnInfo instead")]
        public void SetDawnInfo(DawnItemInfo itemInfo)
        {
            item.SetDawnInfoCore(itemInfo);
        }

        [Obsolete]
        public bool HasDawnInfo()
        {
            return item.DawnInfo != null;
        }

        private DawnItemInfo GetDawnInfoCore()
        {
            return ((IItemDawnObject)item).DawnInfo;
        }

        private void SetDawnInfoCore(DawnItemInfo itemInfo)
        {
            ((IItemDawnObject)item).DawnInfo = itemInfo;
        }
    }
}