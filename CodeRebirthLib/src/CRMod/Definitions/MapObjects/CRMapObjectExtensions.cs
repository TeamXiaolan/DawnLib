using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CodeRebirthLib.CRMod;
using UnityEngine;

namespace CodeRebirthLib;
public static class CRMapObjectExtensions
{
    public static bool TryGetDefinition(this GameObject type, [NotNullWhen(true)] out CRMapObjectDefinition? definition)
    {
        definition = LethalContent.MapObjects.Values.FirstOrDefault(it => it.MapObject == type);
        if (!definition) Debuggers.ReplaceThis?.Log($"TryGetDefinition for MapObjectDefinition failed with {type.name}");
        return definition; // implict cast
    } // todo
}