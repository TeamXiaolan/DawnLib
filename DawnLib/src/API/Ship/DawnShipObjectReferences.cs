using Dawn.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Dawn;
public class DawnShipObjectReferences : MonoBehaviour
{
    //start of round refs
    public Transform outsideShipSpawnPosition;
    public Transform groundOutsideShipSpawnPosition;
    public Transform rightmostSuitPosition;
    public AudioSource ship3DAudio;
    public AudioSource shipDoorAudioSource;
    public Animator shipDoorsAnimator;
    public ShipLights shipRoomLights;
    public AnimatedObjectTrigger closetLeftDoor;
    public AnimatedObjectTrigger closetRightDoor;
    public Transform gameOverCameraHandle;
    public Transform middleOfShipNode;
    public Transform shipDoorNode;
    public Transform middleOfSpaceNode;
    public Transform moveAwayFromShipNode;
    public Collider shipBounds;
    public Collider shipInnerRoomBounds;
    public Collider shipStrictInnerRoomBounds;

    public List<DawnSceneObjectReference> dawnSceneObjectReferences = new();

    public void ReplaceReferences()
    {
        StartOfRoundRefs.Instance.outsideShipSpawnPosition = outsideShipSpawnPosition;
        StartOfRoundRefs.Instance.groundOutsideShipSpawnPosition = groundOutsideShipSpawnPosition;
        StartOfRoundRefs.Instance.rightmostSuitPosition = rightmostSuitPosition;
        StartOfRoundRefs.Instance.ship3DAudio = ship3DAudio;
        StartOfRoundRefs.Instance.shipDoorAudioSource = shipDoorAudioSource;
        StartOfRoundRefs.Instance.shipDoorsAnimator = shipDoorsAnimator;
        StartOfRoundRefs.Instance.shipRoomLights = shipRoomLights;
        StartOfRoundRefs.Instance.closetLeftDoor = closetLeftDoor;
        StartOfRoundRefs.Instance.closetRightDoor = closetRightDoor;
        StartOfRoundRefs.Instance.gameOverCameraHandle = gameOverCameraHandle;
        StartOfRoundRefs.Instance.middleOfShipNode = middleOfShipNode;
        StartOfRoundRefs.Instance.shipDoorNode = shipDoorNode;
        StartOfRoundRefs.Instance.middleOfSpaceNode = middleOfSpaceNode;
        StartOfRoundRefs.Instance.moveAwayFromShipNode = moveAwayFromShipNode;
        StartOfRoundRefs.Instance.shipBounds = shipBounds;
        StartOfRoundRefs.Instance.shipInnerRoomBounds = shipInnerRoomBounds;
        StartOfRoundRefs.Instance.shipStrictInnerRoomBounds = shipStrictInnerRoomBounds;

        foreach (var reference in dawnSceneObjectReferences)
        {
            if (!reference) continue;

            if (!reference.TryFind()) DawnPlugin.Logger.LogWarning($"Cant find {reference.cachedObjectPath}");
            if (!reference.TryMove()) DawnPlugin.Logger.LogWarning($"Cant move {reference.cachedObjectPath}");
        }
    }
}
