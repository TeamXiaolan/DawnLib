using Dawn;
using Dawn.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Dusk;
internal class ShipSpawnHandler
{
    // BE HAPPY PLAN (mel named it)

    //general:
    //load into lobby
    //save vanila stuff into crucialObjectsDict and nonCrucialObjectsDict

    //vanila -> modded:
    //  load and place our cool new ship
    //  replace everything that needs to be replaced in roundmanager/startofround
    //  go thru every component for moving stuff like lever, monitors, magnet lever, etc and move them to positions/rotations/scale/parent from vanilaObjectsDict
    //  disable vanila ship mesh/doors/lights and MAYBE something extra
    //  save
    //  be happy and pray

    //modded -> vanila:
    //  enable vanila ship mesh/doors/lights etc
    //  replace everything back that needs to be replaced in roundmanager/startofround
    //  move every gameobject from vanilaObjectsDict to its original positions
    //  unload our not so cool new ship
    //  save

    //loading lobby:
    //  check current save:
    //    if need to load new ship then vanila -> modded + general sections
    //    if need to load vanila ship then just general section

    //misc: transpiler for costume positions that i still need to move from my repo to damnlib

    //i felt like i was writing an essay

    //objects that i cant remove and should not be changed(except position and etc)
    //any AutoParrentToShip(exception: LightSwitchContainer), RightmostSuitPlacement, StartGameLever, ShipModels2b (?), Cameras (?), MagnetLever, GiantCylinderMagnet
    //other objects should be in nonCrucialObjectDict
    private Dictionary<string, TransformState> crucialObjectsDict = new();

    //when ship is vanila then all objects should be enabled, otherwise - disabled 
    private Dictionary<string, Transform> nonCrucialObjectsDict = new(); 
    
    internal static void Initialize() 
    {
        //fill up crucialObjectsDict
        //read shipid/shipname/whatever from save file
        //change ship
    }

    internal static void ChangeShip(int shipID)
    {
        if (((ITerminal)TerminalRefs.Instance).CurrentShipId == shipID) return;

        if (shipID == 0)
        {
            throw new NotImplementedException(); //modded -> vanila
        }
        else
        {
            throw new NotImplementedException(); //vanila -> modded
        }
    }
}
