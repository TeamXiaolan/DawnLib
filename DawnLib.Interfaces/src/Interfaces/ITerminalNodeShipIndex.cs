using InjectionLibrary.Attributes;

namespace Dawn.Interfaces;

[InjectInterface(typeof(Terminal))]
public interface ITerminalNodeShipIndex
{
    public int buyShipIndex { get; set; }
}
