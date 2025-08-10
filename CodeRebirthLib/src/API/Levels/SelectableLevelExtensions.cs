using System.Globalization;
using System.Linq;

namespace CodeRebirthLib;

public static class SelectableLevelExtensions
{
    public static NamespacedKey<CRMoonInfo> ToNamespacedKey(this SelectableLevel level)
    {
        return NamespacedKey<CRMoonInfo>.Vanilla(new string(level.PlanetName.SkipWhile(c => !char.IsLetter(c)).ToArray()).Replace(" ", "_").ToLower(CultureInfo.InvariantCulture));
    }    
}
