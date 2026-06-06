using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(SelectableLevel))]
public interface ISelectableLevelDawnObject
{
    DawnMoonInfo DawnInfo { get; set; }
}