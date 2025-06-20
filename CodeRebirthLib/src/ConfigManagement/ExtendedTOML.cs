using System.Collections.Generic;
using BepInEx.Configuration;
using CodeRebirthLib.ConfigManagement.Converters;

namespace CodeRebirthLib.ConfigManagement;
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
            TomlTypeConverter.AddConverter(converter.ConvertingType,
                new TypeConverter
                {
                    ConvertToObject = (s, type) => converter.Deserialize(s),
                    ConvertToString = (obj, type) => converter.Serialize(obj),
                }
            );
            CodeRebirthLibPlugin.Logger.LogInfo($"[ExtendedTOML] Registered converter for '{converter.ConvertingType.Name}'");
        }
    }
}