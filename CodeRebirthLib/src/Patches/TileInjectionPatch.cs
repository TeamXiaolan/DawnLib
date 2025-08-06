using System.Collections.Generic;
using CodeRebirthLib.ContentManagement;
using DunGen;
using UnityEngine;

namespace CodeRebirthLib.Patches;
static class TileInjectionPatch
{
    internal static readonly List<GameObject> tilesToFixSockets = new();
    
    internal static void Init()
    {
        
    }

    internal static void FixTileSockets()
    {
        Dictionary<string, DoorwaySocket> mapped = new(); // improve performance
        foreach (DoorwaySocket socket in LethalContent.Dungeons.VanillaDoorSockets)
        {
            mapped[socket.name] = socket;
        }
        
        foreach (GameObject tile in tilesToFixSockets)
        {
            Doorway[] doorways = tile.GetComponentsInChildren<Doorway>();

            foreach (Doorway doorway in doorways)
            {
                doorway.socket = mapped[doorway.socket.name]; // this updates it to use the vanilla reference from the game
            }
        }
    }
}