using InjectionLibrary.Attributes;

namespace Dawn.Interfaces;

[InjectInterface(typeof(Terminal))]
public interface ITerminalBuyableShips
{
    public object buyableShips { get; set; } //supposed to be BuyableShipPreset type but i THINK i cant use that here?
}

