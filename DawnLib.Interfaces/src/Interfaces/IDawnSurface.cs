
using GameNetcodeStuff;
using InjectionLibrary.Attributes;

[assembly: RequiresInjections]

namespace Dawn.Interfaces;
[InjectInterface(typeof(PlayerControllerB))]
[InjectInterface(typeof(MaskedPlayerEnemy))]
public interface IDawnSurface
{
    object? CurrentDawnSurface { get; set; }
}