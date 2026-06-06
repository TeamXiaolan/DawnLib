using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(Item))]
public interface IItemDawnObject
{
    DawnItemInfo DawnInfo { get; set; }
}