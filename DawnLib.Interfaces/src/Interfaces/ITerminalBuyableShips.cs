using Dawn.Preloader;

namespace Dawn.Interfaces;

[InjectInterface("Terminal")]
public interface ITerminalBuyableShips
{
    public object buyableShips { get; set; }
}

