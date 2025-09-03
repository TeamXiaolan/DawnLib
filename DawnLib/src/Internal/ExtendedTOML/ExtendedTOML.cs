using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Dawn.Internal;
static class ExtendedTOML
{
    private static readonly TypeConverter _namespacedKeyConverter = WrapCRLibConverter(new NamespacedKeyConverter());
    private static readonly List<TOMLConverter> _converters =
    [
        new BoundedRangeConverter(),
        new AnimationCurveConverter(),
        new NamespacedKeyConverter()
    ];

    internal static void Init()
    {
        foreach (TOMLConverter converter in _converters)
        {
            if (!converter.IsEnabled())
            {
                Debuggers.ExtendedTOML?.Log($"[ExtendedTOML] Skipped converter for '{converter.ConvertingType.Name}' it disabled itself.");
                continue;
            }

            TomlTypeConverter.AddConverter(converter.ConvertingType, WrapCRLibConverter(converter));
            DawnPlugin.Logger.LogInfo($"[ExtendedTOML] Registered converter for '{converter.ConvertingType.Name}'");
        }

        On.BepInEx.Configuration.TomlTypeConverter.GetConverter += SupplyNamespacedKeyConverter;

        // untested and should probably be behind a config option
        // IL.BepInEx.Configuration.ConfigEntryBase.WriteDescription += PrettyPrintConfigSettingType;
    }
    private static void PrettyPrintConfigSettingType(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(MoveType.After,
            i => i.MatchLdarg(1),
            i => i.MatchLdstr("# Setting type: "),
            i => i.MatchLdarg(0),
            i => i.MatchCall<ConfigEntryBase>("get_SettingType")
        );
        c.Next = Instruction.Create(OpCodes.Nop);
        c.EmitDelegate(PrettyTypeName);
    }

    static string PrettyTypeName(Type t)
    {
        if (t.IsArray)
        {
            return PrettyTypeName(t.GetElementType()) + "[]";
        }

        if (t.IsGenericType)
        {
            return string.Format(
                "{0}<{1}>",
                t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.InvariantCulture)),
                string.Join(", ", t.GetGenericArguments().Select(PrettyTypeName))
            );
        }

        return t.Name;
    }

    // this automatically handles generic types of NamespacedKey eg. NamespacedKey<MyCustomInfo> is handled automatically properly.
    private static TypeConverter SupplyNamespacedKeyConverter(On.BepInEx.Configuration.TomlTypeConverter.orig_GetConverter orig, Type valuetype)
    {
        if (typeof(NamespacedKey).IsAssignableFrom(valuetype))
        {
            return _namespacedKeyConverter;
        }
        return orig(valuetype);
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