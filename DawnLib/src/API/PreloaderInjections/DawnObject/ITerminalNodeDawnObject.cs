using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(TerminalNode))]
public interface ITerminalNodeDawnObject
{
    DawnTerminalCommandInfo DawnInfo { get; set; }
}