using Dawn;
using Dawn.Internal;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dusk;
public class ShipSpawnHandler
{
    /*
    private static ShipSpawnHandler instance = null!;

    private ShipSpawnHandler() {}
    public static ShipSpawnHandler Instance
    {
        get
        {
            if (instance == null) instance = new ShipSpawnHandler();
            return instance;
        }
    }
    */

    public ShipSpawnHandler() { }

    /*
    * BE HAPPY PLAN (mel named it)
    *
    * general:
    * load into lobby
    * save vanila stuff into crucialObjectsDict and nonCrucialObjectsDict
    *
    * vanila -> modded:
    *   load and place our cool new ship
    *   replace everything that needs to be replaced in roundmanager/startofround
    *   go thru every component for moving stuff like lever, monitors, magnet lever, etc and move them to positions/rotations/scale/parent from vanilaObjectsDict
    *   disable vanila ship mesh/doors/lights and MAYBE something extra
    *   save
    *   be happy and pray
    *
    * modded -> vanila:
    *   enable vanila ship mesh/doors/lights etc
    *   replace everything back that needs to be replaced in roundmanager/startofround
    *   move every gameobject from vanilaObjectsDict to its original positions
    *   unload our not so cool new ship
    *   save
    *
    * loading lobby:
    *   check current save:
    *     if need to load new ship then vanila -> modded + general sections
    *     if need to load vanila ship then just general section
    *
    * misc: transpiler for costume positions that i still need to move from my repo to damnlib
    *
    * i felt like i was writing an essay
    */

    //objects that i cant remove and should not be changed(except position and etc)
    //any AutoParrentToShip(exception?: LightSwitchContainer), RightmostSuitPlacement, StartGameLever, ShipModels2b (?), Cameras (?), MagnetLever, GiantCylinderMagnet
    //other objects should be in nonCrucialObjectDict
    public Dictionary<string, DawnSceneObjectReference> crucialObjectsDict = new();

    //when ship is vanila then all objects should be enabled, otherwise - disabled 
    public Dictionary<string, GameObject> nonCrucialObjectsDict = new();

    public NetworkObject newNetworkShipObjects;

    public void Initialize()
    {
        #region fill up crucialObjectsDict
        var autoParentToShipArray = GameObject.FindObjectsByType<AutoParentToShip>(FindObjectsSortMode.InstanceID);
        var suitPosition = StartOfRoundRefs.Instance.rightmostSuitPosition;
        var shipLever = StartMatchLeverRefs.Instance.transform;
        var monitors = StartOfRoundRefs.Instance.mapScreen.transform.parent.parent;
        var cameras = StartOfRoundRefs.Instance.shipAnimatorObject.transform.Find("Cameras"); //i couldnt find where i could get a reference without Find
        var magnetLever = StartOfRoundRefs.Instance.magnetLever.transform;
        var magnet = StartOfRoundRefs.Instance.magnetPoint.parent;

        List<Transform> list = new();
        foreach (var obj in autoParentToShipArray)
            list.Add(obj.transform);
        list.Add(suitPosition);
        list.Add(shipLever);
        list.Add(monitors);
        list.Add(cameras);
        list.Add(magnetLever);
        list.Add(magnet);

        foreach (var gotransform in list)
        {
            var temp = gotransform.gameObject.AddComponent<DawnSceneObjectReference>();
            temp.foundObject = gotransform;
            crucialObjectsDict.Add(temp.name, temp);
        }

        #endregion

        #region fill up nonCrucialObjectsDict
        foreach (Transform obj in StartOfRoundRefs.Instance.shipAnimatorObject.transform)
        {
            bool skip = false;
            if (crucialObjectsDict.ContainsKey(obj.name)) continue;

            //no need to go deeper
            foreach (Transform child in obj)
            {
                if (crucialObjectsDict.ContainsKey(child.name))
                {
                    skip = true;
                    continue;
                }

                nonCrucialObjectsDict.Add(child.name, child.gameObject);
            }

            if (!skip) nonCrucialObjectsDict.Add(obj.name, obj.gameObject);
        }
        #endregion
    }

    public NamespacedKey LoadShipFromSave()
    {
        PersistentDataContainer? save = DawnLib.GetCurrentSave();
        NamespacedKey currentShipKey = NamespacedKey.From("dawn_lib", "current_ship");
        NamespacedKey? shipKey = NamespacedKey.From("lethal_company", "ship");

        if (save != null)
            save.TryGet(currentShipKey, out shipKey);

        return shipKey == null ? NamespacedKey.From("lethal_company", "ship") : shipKey;
    }

    public void ChangeShip(NamespacedKey namespacedKey)
    {
        Debuggers.Ships?.Log(namespacedKey.ToString());
        if (((ITerminal)TerminalRefs.Instance).CurrentShip == namespacedKey.ToString()) return;
        if (!StartOfRoundRefs.Instance.inShipPhase) return;
        //TODO: add price check

        if (namespacedKey.IsVanilla())
        {
            foreach (var go in nonCrucialObjectsDict.Values)
                go.SetActive(true);

            newNetworkShipObjects.Despawn();
        }
        else
        {
            newNetworkShipObjects = LethalContent.Ships[namespacedKey.AsTyped<DawnShipInfo>()].ShipPrefab.GetComponent<NetworkObject>();
            newNetworkShipObjects.Spawn();

            foreach (var go in nonCrucialObjectsDict.Values)
                go.SetActive(false);
        }
    }
}
