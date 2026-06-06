using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;
using Dawn.Utils;

namespace Dawn;

public static class MaskedPlayerEnemyExtensions
{
    public static bool TryGetCurrentDawnSurface(this MaskedPlayerEnemy maskedEnemy, [NotNullWhen(true)] out DawnSurface? dawnSurface)
    {
        dawnSurface = (DawnSurface?)((ICurrentDawnSurface)maskedEnemy).CurrentDawnSurface;
        return dawnSurface != null;
    }

    public static void SetCurrentDawnSurface(this MaskedPlayerEnemy maskedEnemy, DawnSurface? dawnSurface)
    {
        ((ICurrentDawnSurface)maskedEnemy).CurrentDawnSurface = dawnSurface;
    }
}