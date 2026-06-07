using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;

[InjectInterface(typeof(RoundManager))]
interface IRoundManagerInjects
{
    int CurrentDaytimeDiversity { get; set; }
    int CurrentDaytimeMaxDiversity { get; set; }
    int CurrentWeedDiversity { get; set; }
    int CurrentWeedMaxDiversity { get; set; }

    float CurrentDaytimeMaxPower { get; set; }
    float CurrentWeedMaxPower { get; set; }
}