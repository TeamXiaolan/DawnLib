using System;
using Dawn.Interfaces;

namespace Dawn;

public static class UnlockableItemExtensions
{
    extension(UnlockableItem unlockableItem)
    {
        public DawnUnlockableItemInfo DawnInfo
        {
            get => unlockableItem.GetDawnInfoCore();
            set => unlockableItem.SetDawnInfoCore(value);
        }

        [Obsolete("Use Item.DawnInfo instead")]
        public DawnUnlockableItemInfo GetDawnInfo()
        {
            return unlockableItem.GetDawnInfoCore();
        }

        private DawnUnlockableItemInfo GetDawnInfoCore()
        {
            return ((IUnlockableItemDawnObject)unlockableItem).DawnInfo;
        }

        private void SetDawnInfoCore(DawnUnlockableItemInfo unlockableItemInfo)
        {
            ((IUnlockableItemDawnObject)unlockableItem).DawnInfo = unlockableItemInfo;
        }
    }
}