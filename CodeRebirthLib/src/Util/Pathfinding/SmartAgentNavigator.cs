using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathfindingLib.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace CodeRebirthLib.Util.Pathfinding;
[RequireComponent(typeof(NavMeshAgent))]
public class SmartAgentNavigator : NetworkBehaviour
{
    [HideInInspector] public bool cantMove = false;
    [HideInInspector] public UnityEvent<bool> OnUseEntranceTeleport = new();
    [HideInInspector] public UnityEvent<bool> OnEnableOrDisableAgent = new();
    [HideInInspector] public UnityEvent<bool> OnEnterOrExitElevator = new();
    [HideInInspector] public EntranceTeleport lastUsedEntranceTeleport;

    private float nonAgentMovementSpeed = 10f;
    [HideInInspector] public NavMeshAgent agent = null!;
    private Vector3 pointToGo = Vector3.zero;
    [HideInInspector] public bool isOutside = true;
    private bool usingElevator = false;
    private bool inElevator = false;
    private Coroutine? searchRoutine = null;
    private bool isSearching = false;
    private bool reachedDestination = false;
    private MineshaftElevatorController? elevatorScript = null;
    [HideInInspector] public Coroutine? checkPathsRoutine = null;
    [HideInInspector] public PathfindingOperation? pathfindingOperation = null;
    [HideInInspector] public List<EntranceTeleport> exitPoints = new();
    [HideInInspector] public EntranceTeleport? mainEntrance = null;

    public void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    public void SetAllValues(bool isOutside)
    {
        this.isOutside = isOutside;

        exitPoints.Clear(); // todo: Replace this findobjectsoftype with something cached
        foreach (var exit in FindObjectsOfType<EntranceTeleport>())
        {
            exitPoints.Add(exit);
            if (exit.entranceId == 0 && exit.isEntranceToBuilding)
            {
                mainEntrance = exit;
            }
            // Plugin.ExtendedLogging($"Exit point Entrance: {exit.entrancePoint.position} Exit: {exit.exitPoint.position} and are Entrances: {exit.isEntranceToBuilding}");
        }
        elevatorScript = RoundManager.Instance.currentMineshaftElevator;
    }

    public void ResetAllValues()
    {
        exitPoints.Clear();
        elevatorScript = null;
        mainEntrance = null;
    }

    public void StopAgent()
    {
        if (agent.enabled && agent.isOnNavMesh)
            agent.ResetPath();

        agent.velocity = Vector3.zero;
    }

    public bool DoPathingToDestination(Vector3 destination)
    {
        if (isSearching)
        {
            StopSearchRoutine();
        }
        if (cantMove)
            return false;

        if (!agent.enabled)
        {
            HandleDisabledAgentPathing();
            return false;
        }
        return GoToDestination(destination);
    }

    public void CheckPaths<T>(IEnumerable<(T, Vector3)> points, Action<List<T>> action)
    {
        if (checkPathsRoutine != null)
        {
            StopCoroutine(checkPathsRoutine);
            ClearPathfindingOperation();
        }
        checkPathsRoutine = StartCoroutine(CheckPathsCoroutine(points, action));
    }

    private IEnumerator CheckPathsCoroutine<T>(IEnumerable<(T, Vector3)> points, Action<List<T>> action)
    {
        CodeRebirthLibPlugin.ExtendedLogging($"Checking paths for {points.Count()} objects");
        var TList = new List<T>();
        ClearPathfindingOperation();
        foreach (var (obj, point) in points.ToArray())
        {
            bool pathFound = false;
            while (!TryFindViablePath(point, out pathFound, out _))
                yield return null; // Wait for the path to be finished calculating

            if (pathFound)
            {
                TList.Add(obj);
            }
        }
        ClearPathfindingOperation();
        action(TList);
        checkPathsRoutine = null;
    }

    private void HandleDisabledAgentPathing()
    {
        Vector3 targetPosition = pointToGo;
        float arcHeight = 10f;  // Adjusted arc height for a more pronounced arc
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) <= 1f)
        {
            OnEnableOrDisableAgent.Invoke(true);
            agent.enabled = true;
            agent.Warp(targetPosition);
            return;
        }

        // Calculate the new position in an arcing motion
        float normalizedDistance = Mathf.Clamp01(Vector3.Distance(transform.position, targetPosition) / distanceToTarget);
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * nonAgentMovementSpeed);
        newPosition.y += Mathf.Sin(normalizedDistance * Mathf.PI) * arcHeight;

        transform.SetPositionAndRotation(newPosition, Quaternion.LookRotation(targetPosition - transform.position));
    }

    private bool DetermineIfNeedToDisableAgent(Vector3 destination)
    {
        float distanceToDest = Vector3.Distance(transform.position, destination);
        if (distanceToDest <= agent.stoppingDistance + 5f)
        {
            return false;
        }

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 3, agent.areaMask))
        {
            return false;
        }
        Vector3 lastValidPoint = FindClosestValidPoint();
        agent.SetDestination(lastValidPoint);
        if (Vector3.Distance(agent.transform.position, lastValidPoint) <= agent.stoppingDistance)
        {
            pointToGo = hit.position;
            OnEnableOrDisableAgent.Invoke(false);
            agent.enabled = false;
            CodeRebirthLibPlugin.ExtendedLogging($"Pathing to initial destination {destination} failed, going to fallback position {hit.position} instead.");
            return true;
        }

        return false;
    }

    public float CanPathToPoint(Vector3 startPos, Vector3 endPos)
    {
        NavMeshPath path = new NavMeshPath();
        bool pathFound = NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path);

        if (!pathFound || path.status != NavMeshPathStatus.PathComplete)
        {
            return -1;
        }

        float pathDistance = 0f;
        if (path.corners.Length > 1)
        {
            for (int i = 1; i < path.corners.Length; i++)
            {
                pathDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }
        // Plugin.ExtendedLogging($"[{this.gameObject.name}] Path distance: {pathDistance}");

        return pathDistance;
    }

    public void ClearPathfindingOperation()
    {
        pathfindingOperation?.Dispose();
        pathfindingOperation = null;
    }

    internal T ChangePathfindingOperation<T>(Func<T> provider) where T : PathfindingOperation
    {
        if (pathfindingOperation?.HasDisposed() ?? false)
            pathfindingOperation = null;
        if (pathfindingOperation is not T specificOperation)
        {
            specificOperation = provider();
            pathfindingOperation?.Dispose();
            pathfindingOperation = specificOperation;
        }
        return specificOperation;
    }

    public bool TryFindViablePath(Vector3 endPosition, out bool foundPath, out EntranceTeleport? entranceTeleport)
    {
        var findPathThroughTeleportsOperation = ChangePathfindingOperation(() => new FindPathThroughTeleportsOperation(exitPoints, agent.GetPathOrigin(), endPosition, agent));
        return findPathThroughTeleportsOperation.TryGetShortestPath(out foundPath, out entranceTeleport);
    }

    public bool GoToDestination(Vector3 actualEndPosition)
    {
        // Attempt to find an entrance that’s viable for (object -> entrance) and (entrance -> agent).
        if (TryFindViablePath(actualEndPosition, out bool foundPath, out EntranceTeleport? entranceToUse))
        {
            if (entranceToUse == null && !foundPath) // still null after calculating
            {
                if (elevatorScript != null)
                {
                    if (NeedsElevator(actualEndPosition, elevatorScript, out bool goingUp))
                    {
                        usingElevator = true;
                        HandleElevatorActions(elevatorScript, goingUp);
                        return false;
                    }
                    else if (!elevatorScript.elevatorFinishedMoving && Vector3.Distance(actualEndPosition, elevatorScript.elevatorInsidePoint.position) < 7f)
                    {
                        return false;
                    }
                }

                if ((isOutside && actualEndPosition.y > -50) || (!isOutside && actualEndPosition.y < -50))
                {
                    DetermineIfNeedToDisableAgent(actualEndPosition);
                    return false;
                }
                if (mainEntrance == null)
                {
                    return false;
                }
                DoPathingThroughEntrance(mainEntrance);
                // fallback?
                return false;
            }
            else if (entranceToUse == null)
            {
                // Plugin.ExtendedLogging($"No entrance found, but path found");
                agent.SetDestination(actualEndPosition);
                return true;
            }
        }
        else
        {
            // Plugin.ExtendedLogging($"Ongoing calculation, returning early");
            return false;
        }

        DoPathingThroughEntrance(entranceToUse);
        return false;
    }

    private void HandleElevatorActions(MineshaftElevatorController elevatorScript, bool goingUp)
    {
        if (inElevator)
        {
            agent.Warp(elevatorScript.elevatorInsidePoint.position);
            return;
        }
        UseTheElevator(elevatorScript, goingUp);
        return;
    }

    private void DoPathingThroughEntrance(EntranceTeleport viableEntrance)
    {
        Vector3 destination = viableEntrance.entrancePoint.position;
        Vector3 destinationAfterTeleport = viableEntrance.exitPoint.position;

        float distanceToDestination = Vector3.Distance(transform.position, destination);
        // Plugin.ExtendedLogging($"Distance to destination: {distanceToDestination} to destination: {destination}");
        if (distanceToDestination <= agent.stoppingDistance + 1f)
        {
            agent.Warp(destinationAfterTeleport);
            SetThingOutsideServerRpc(!isOutside, new NetworkBehaviourReference(viableEntrance));
            return;
        }
        else
        {
            // Otherwise, set the path to that entrance
            agent.SetDestination(destination);
        }
    }

    private bool NeedsElevator(Vector3 destination, MineshaftElevatorController elevatorScript, out bool goingUp)
    {
        goingUp = false;
        if (isOutside && destination.y > -50)
        {
            usingElevator = false;
            return false;
        }
        if (usingElevator) return true;
        // Determine if the elevator is needed based on destination proximity and current position
        bool destinationCloserToMainEntrance = Vector3.Distance(destination, RoundManager.FindMainEntrancePosition(true, false)) < Vector3.Distance(destination, elevatorScript.elevatorBottomPoint.position);
        bool notCloseToTopPoint = Vector3.Distance(transform.position, elevatorScript.elevatorTopPoint.position) > 15f;
        goingUp = destinationCloserToMainEntrance;
        return (destinationCloserToMainEntrance && notCloseToTopPoint) || (!notCloseToTopPoint && !destinationCloserToMainEntrance);
    }

    private void UseTheElevator(MineshaftElevatorController elevatorScript, bool goingUp)
    {
        // Check if the elevator is finished moving
        MoveToWaitingPoint(elevatorScript, goingUp);
        if (elevatorScript.elevatorFinishedMoving)
        {
            if (!elevatorScript.elevatorDoorOpen) return;
            // If elevator is not called yet and is at the wrong level, call it
            if (NeedToCallElevator(elevatorScript, goingUp))
            {
                elevatorScript.CallElevatorOnServer(goingUp);
                return;
            }
            // Move to the inside point of the elevator if not already there
            if (Vector3.Distance(transform.position, elevatorScript.elevatorInsidePoint.position) > 1f)
            {
                agent.SetDestination(elevatorScript.elevatorInsidePoint.position);
            }
            else if (!inElevator)
            {
                // Press the button to start moving the elevator
                elevatorScript.PressElevatorButtonOnServer(true);
                StartCoroutine(StopUsingElevator(elevatorScript));
            }
        }
    }

    private IEnumerator StopUsingElevator(MineshaftElevatorController elevatorScript)
    {
        inElevator = true;
        OnEnterOrExitElevator.Invoke(true);
        yield return new WaitForSeconds(2f);
        yield return new WaitUntil(() => elevatorScript.elevatorDoorOpen && elevatorScript.elevatorFinishedMoving);
        CodeRebirthLibPlugin.ExtendedLogging("Stopped using elevator");
        usingElevator = false;
        OnEnterOrExitElevator.Invoke(false);
        inElevator = false;
    }

    private bool NeedToCallElevator(MineshaftElevatorController elevatorScript, bool needToGoUp)
    {
        return !elevatorScript.elevatorCalled && ((!elevatorScript.elevatorIsAtBottom && needToGoUp) || (elevatorScript.elevatorIsAtBottom && !needToGoUp));
    }

    private void MoveToWaitingPoint(MineshaftElevatorController elevatorScript, bool needToGoUp)
    {
        // Elevator is currently moving
        // Move to the appropriate waiting point (bottom or top)
        if (Vector3.Distance(transform.position, elevatorScript.elevatorInsidePoint.position) > 1f)
        {
            agent.SetDestination(needToGoUp ? elevatorScript.elevatorBottomPoint.position : elevatorScript.elevatorTopPoint.position);
        }
        else
        {
            // Wait at the inside point for the elevator to arrive
            agent.SetDestination(elevatorScript.elevatorInsidePoint.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetThingOutsideServerRpc(bool setOutside, NetworkBehaviourReference entranceTeleportReference)
    {
        SetThingOutsideClientRpc(setOutside, entranceTeleportReference);
    }

    [ClientRpc]
    public void SetThingOutsideClientRpc(bool setOutside, NetworkBehaviourReference entranceTeleportReference)
    {
        isOutside = setOutside;
        lastUsedEntranceTeleport = (EntranceTeleport)entranceTeleportReference;
        OnUseEntranceTeleport.Invoke(setOutside);
    }

    /// <summary>
    /// Finds the closest valid point from a partial path.
    /// </summary>
    /// <returns>The closest valid point to continue the journey from.</returns>
    private Vector3 FindClosestValidPoint()
    {
        return agent.pathEndPosition;
    }

    /// <summary>
    /// Warps the agent forward in its current direction until it lands on a valid NavMesh.
    /// </summary>
    /// <param name="originalDestination">The target destination outside the navmesh.</param>
    /// <returns>The point on the navmesh.</returns>
    private Vector3 WarpForwardUntilOnNavMesh(Vector3 originalDestination)
    {
        Vector3 warpPoint = originalDestination;
        Vector3 forwardDirection = agent.transform.forward; // Get the agent's current facing direction
        float warpDistance = 0.2f; // The distance to warp forward each time
        float maxWarpAttempts = 50; // Limit the number of warp attempts
        float navMeshCheckDistance = 1f; // The radius to check for a valid NavMesh

        for (int i = 0; i < maxWarpAttempts; i++)
        {
            Vector3 testPoint = warpPoint + forwardDirection * warpDistance * (i + 1);

            // Check if this new point is on the NavMesh
            if (NavMesh.SamplePosition(testPoint, out NavMeshHit hit, navMeshCheckDistance, NavMesh.AllAreas))
            {
                // Found a valid point on the NavMesh
                return hit.position;
            }
        }

        // If no valid point is found after multiple attempts, return zero vector
        CodeRebirthLibPlugin.Logger.LogWarning("Unable to find valid point on NavMesh by warping forward.");
        return Vector3.zero;
    }

    /// <summary>
    /// Adjusts the agent's speed based on its remaining distance.
    /// The farther the distance, the faster the agent moves.
    /// </summary>
    /// <param name="distance">The distance to the target.</param>
    public void AdjustSpeedBasedOnDistance(float multiplierBoost)
    {
        float minDistance = 0f;
        float maxDistance = 40f;

        float minSpeed = 0f; // Speed when closest
        float maxSpeed = 10f; // Speed when farthest

        float clampedDistance = Mathf.Clamp(agent.remainingDistance, minDistance, maxDistance);
        float normalizedDistance = (clampedDistance - minDistance) / (maxDistance - minDistance);

        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, normalizedDistance) * multiplierBoost;
    }

    /// <summary>
    /// Cancels the current movement and stops the agent.
    /// </summary>
    public void StopNavigation()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    /// <summary>
    /// Teleports the agent to a specified location.
    /// </summary>
    /// <param name="location">The target location to warp to.</param>
    public void WarpToLocation(Vector3 location)
    {
        agent.Warp(location);
    }

    public bool CurrentPathIsValid()
    {
        if (agent.path.status == NavMeshPathStatus.PathPartial || agent.path.status == NavMeshPathStatus.PathInvalid)
        {
            return false;
        }
        return true;
    }

    public void StartSearchRoutine(Vector3 position, float radius)
    {
        if (!agent.enabled)
            return;

        StopSearchRoutine();
        isSearching = true;
        searchRoutine = StartCoroutine(SearchAlgorithm(position, radius));
    }

    public void StopSearchRoutine()
    {
        isSearching = false;
        if (searchRoutine != null)
        {
            StopCoroutine(searchRoutine);
        }
        StopAgent();
        searchRoutine = null;
    }

    private IEnumerator SearchAlgorithm(Vector3 position, float radius)
    {
        while (isSearching)
        {
            Vector3 positionToTravel = RoundManager.Instance.GetRandomNavMeshPositionInRadius(position, radius, default);
            // Plugin.ExtendedLogging($"Search: {positionToTravel}");
            reachedDestination = false;

            while (!reachedDestination && isSearching)
            {
                CodeRebirthLibPlugin.ExtendedLogging($"Search: {positionToTravel}");
                agent.SetDestination(positionToTravel);
                yield return new WaitForSeconds(3f);

                if (!agent.enabled || Vector3.Distance(this.transform.position, positionToTravel) <= 10f || agent.velocity.magnitude <= 1f)
                {
                    reachedDestination = true;
                }
            }
        }

        searchRoutine = null;
    }
}