using System.Diagnostics.CodeAnalysis;

namespace CodeRebirthLib.ContentManagement.MapObjects;
public static class CRModMapObjectExtensions
{
    public static bool TryGetFromMapObjectName(this CRRegistry<CRMapObjectDefinition> registry, string mapObjectName, [NotNullWhen(true)] out CRMapObjectDefinition? value)
    {
        return registry.TryGetFirstBySomeName(
            it => it.ObjectName, 
            mapObjectName, 
            out value, 
            $"TryGetFromMapObjectName failed with mapObjectName: {mapObjectName}"
        );
    }

    /*public static bool TryGetDefinition(this MapObjectType type, [NotNullWhen(true)] out CRMapObjectDefinition? definition)
    {
        definition = CRMod.AllMapObjects().FirstOrDefault(it => it.MapObjectType == type);
        return definition; // implict cast
    }*/ // todo
}