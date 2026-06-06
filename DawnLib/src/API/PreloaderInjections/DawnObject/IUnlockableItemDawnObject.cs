using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(UnlockableItem))]
public interface IUnlockableItemDawnObject
{
    DawnUnlockableItemInfo DawnInfo { get; set; }
}