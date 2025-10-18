using Dawn.Interfaces;
using DunGen;

namespace Dawn;

public static class DungeonArchetypeExtensions
{
    public static DawnArchetypeInfo GetDawnInfo(this DungeonArchetype archetype)
    {
        object newObject = archetype;
        DawnArchetypeInfo archetypeInfo = (DawnArchetypeInfo)((IDawnObject)newObject).DawnInfo;
        return archetypeInfo;
    }

    internal static bool HasDawnInfo(this DungeonArchetype archetype)
    {
        return archetype.GetDawnInfo() != null;
    }

    internal static void SetDawnInfo(this DungeonArchetype archetype, DawnArchetypeInfo archetypeInfo)
    {
        object newObject = archetype;
        ((IDawnObject)newObject).DawnInfo = archetypeInfo;
    }
}
