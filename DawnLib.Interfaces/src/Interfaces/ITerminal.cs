using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn;

[InjectInterface(typeof(Terminal))]
public interface ITerminal
{
    //Used by TerminalKeywords that accept input to determine the last keyword used
    string DawnLastCommand { get; set; }

    //Can be used to determine the last keyword resolved by the terminal
    TerminalKeyword DawnLastNoun { get; set; }

    //Used by keyword matching
    TerminalKeyword DawnLastVerb { get; set; }


    public object BuyableShips { get; set; } //supposed to be BuyableShipPreset type but i THINK i cant use that here?

    public int CurrentShipId { get; set; }
}
