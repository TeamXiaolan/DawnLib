using System.Collections.Generic;
using CodeRebirthLib.ContentManagement;
using DunGen;
using IL.DunGen.Graph;
using UnityEngine;
using DungeonFlow = DunGen.Graph.DungeonFlow;

namespace CodeRebirthLib.Patches;
static class TileInjectionPatch
{
    internal class TileInjectionSettings(TileSet set, bool isBranchCap)
    {
        public TileSet Set { get; } = set;
        public bool IsBranchCap { get; } = isBranchCap;
    }

    internal static readonly List<GameObject> tilesToFixSockets = [];
    static readonly Dictionary<string, List<TileInjectionSettings>> setsToInjectToArchetypes = [];

    internal static void Init()
    {
        On.DunGen.RuntimeDungeon.Generate += (orig, self) =>
        {
            FixTileSockets();
            TryInjectTileSets(self.Generator.DungeonFlow);
            orig(self);
        };
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

        tilesToFixSockets.Clear();
    }

    internal static void AddTileSetForDungeon(string archetypeName, TileInjectionSettings tileSet)
    {
        if (!setsToInjectToArchetypes.TryGetValue(archetypeName, out List<TileInjectionSettings> sets))
        {
            sets = [];
        }
        sets.Add(tileSet);
        setsToInjectToArchetypes[archetypeName] = sets;
    }

    internal static void TryInjectTileSets(DungeonFlow flow)
    {
        foreach (DungeonArchetype archetype in flow.GetUsedArchetypes())
        {
            if (!setsToInjectToArchetypes.TryGetValue(archetype.name, out List<TileInjectionSettings> toInject))
                continue;

            CodeRebirthLibPlugin.ExtendedLogging($"Injecting {toInject.Count} tileset(s) into {archetype.name}");

            foreach (TileInjectionSettings tileSet in toInject)
            {
                if (tileSet.IsBranchCap)
                {
                    archetype.BranchCapTileSets.Add(tileSet.Set);
                }
                else
                {
                    archetype.TileSets.Add(tileSet.Set);
                }
            }

            // to prevent injecting tile sets multiple times, clear the list
            setsToInjectToArchetypes[archetype.name].Clear();
            setsToInjectToArchetypes[archetype.name].Capacity = 0;
        }
    }
}