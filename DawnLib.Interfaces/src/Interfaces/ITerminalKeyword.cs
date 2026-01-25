using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn;

[InjectInterface(typeof(TerminalKeyword))]
public interface ITerminalKeyword
{
    public enum DawnKeywordType
    {
        Unset = 0, //used for default value before it has been set, ie. vanilla keywords or keywords added outside of DawnLib
        Core, //core vanilla keywords that overwriting could severely affect gameplay
        Code, //doors, turrets, etc.
        Moons,
        Vehicles,
        Store,
        Bestiary,
        SigurdLog,
        TerminalEvent, //Will contain commands created by LLL, WeatherRegistry, etc.
        DawnCommand, //Standard DawnLib-made commands
        Other
    }

    //used to resolve conflicts between keywords of same word
    public DawnKeywordType DawnKeywordPriority { get; set; }

    //Determines if keyword should load it's result if additional text is detected after the keyword
    public bool DawnAcceptAdditionalText { get; set; }

    //Placeholder value, can be used to describe what a command does (modded/vanilla)
    public string DawnKeywordDescription { get; set; }

    //Placeholder value, can be used in the future to categorize commands (modded/vanilla)
    public string DawnKeywordCategory { get; set; }

    //Both Category/Description could be used in an interactive style menu listing all possible keywords that can be run from the terminal
}
