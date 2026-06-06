using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;
using Dawn.Utils;
using GameNetcodeStuff;

namespace Dawn;

public static class PlayerControllerBExtensions
{
    public static bool TryGetCurrentDawnSurface(this PlayerControllerB player, [NotNullWhen(true)] out DawnSurface? dawnSurface)
    {
        dawnSurface = (DawnSurface?)((ICurrentDawnSurface)player).CurrentDawnSurface;
        return dawnSurface != null;
    }

    public static void SetCurrentDawnSurface(this PlayerControllerB player, DawnSurface? dawnSurface)
    {
        ((ICurrentDawnSurface)player).CurrentDawnSurface = dawnSurface;
    }
}