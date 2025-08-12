using System.Collections.Generic;
using BepInEx.Configuration;
using CodeRebirthLib.CRMod.Converters;

namespace CodeRebirthLib.CRMod;
static class ExtendedTOML
{
    private static readonly List<TOMLConverter> _converters =
    [
        new BoundedRangeConverter(),
        new AnimationCurveConverter(),
    ];

    internal static void Init()
    {
        foreach (TOMLConverter converter in _converters)
        {
            if (!converter.IsEnabled())
            {
                Debuggers.ReplaceThis?.Log($"[ExtendedTOML] Skipped converter for '{converter.ConvertingType.Name}' it disabled itself.");
                continue;
            }

            TomlTypeConverter.AddConverter(converter.ConvertingType, WrapCRLibConverter(converter));
            CodeRebirthLibPlugin.Logger.LogInfo($"[ExtendedTOML] Registered converter for '{converter.ConvertingType.Name}'");
        }
    }

    internal static TypeConverter WrapCRLibConverter(TOMLConverter converter)
    {
        return new TypeConverter
        {
            ConvertToObject = (s, type) => converter.Deserialize(s),
            ConvertToString = (obj, type) => converter.Serialize(obj),
        };
    }
}