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