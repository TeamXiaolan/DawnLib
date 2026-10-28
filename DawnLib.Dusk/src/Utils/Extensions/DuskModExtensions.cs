using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dusk.Utils;

public static class DuskModExtensions
{
    public static bool TryGetDuskMod(this IEnumerable<DuskMod> allMods, string pluginGuid, [NotNullWhen(true)] out DuskMod? duskMod)
    {
        duskMod = null;
        foreach (DuskMod mod in allMods)
        {
            if (mod.Plugin.GUID != pluginGuid)
            {
                continue;
            }

            duskMod = mod;
            return true;
        }

        return false;
    }
}