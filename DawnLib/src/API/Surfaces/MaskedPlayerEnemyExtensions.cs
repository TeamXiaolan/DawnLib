using System.Diagnostics.CodeAnalysis;
using Dawn.Interfaces;
using Dawn.Utils;

namespace Dawn;

public static class MaskedPlayerEnemyExtensions
{
    public static bool TryGetCurrentDawnSurface(this MaskedPlayerEnemy maskedEnemy, [NotNullWhen(true)] out DawnSurface? dawnSurface)
    {
        dawnSurface = (DawnSurface?)((IDawnSurface)maskedEnemy).CurrentDawnSurface;
        return dawnSurface != null;
    }

    public static void SetCurrentDawnSurface(this MaskedPlayerEnemy maskedEnemy, DawnSurface? dawnSurface)
    {
        ((IDawnSurface)maskedEnemy).CurrentDawnSurface = dawnSurface;
    }
}