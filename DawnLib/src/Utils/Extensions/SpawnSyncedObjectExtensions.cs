using DunGen;
using UnityEngine;

namespace Dawn.Utils;
public static class SpawnSyncedObjectExtensions
{
    public static string GetPathInTilePrefab(this SpawnSyncedObject sso)
    {
        Transform current = sso.transform;
        string path = current.name;

        while (current.parent != null && !current.TryGetComponent<Tile>(out var _))
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}
