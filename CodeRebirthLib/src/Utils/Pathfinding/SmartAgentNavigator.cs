using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathfindingLib.API.SmartPathfinding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace CodeRebirthLib;

public struct GenericPath<T>(T generic, float pathLength)
{
    public T Generic = generic;
    public float PathLength = pathLength;
}

[RequireComponent(typeof(NavMeshAgent))]
public class SmartAgentNavigator : NetworkBehaviour // TODO: make this not a network behaviour, maybe just have the few rpc's be in a network manager instead
{
    [HideInInspector] public bool cantMove = false;
    [HideInInspector] public UnityEvent<bool> OnUseEntranceTeleport = new();
    [HideInInspector] public UnityEvent<bool> OnEnableOrDisableAgent = new();
    [HideInInspector] public UnityEvent<bool> OnEnterOrExitElevator = new();
    [HideInInspector] public EntranceTeleport? lastUsedEntranceTeleport = null;

    private Vector3 pointToGo = Vector3.zero;
    private Coroutine? searchRoutine = null;

    private SmartPathTask? pathingTask = null;
    private SmartPathTask? checkPathsTask = null;
    private SmartPathTask? roamingTask = null;
    private AgentState _agentState = AgentState.NotSet;

    [HideInInspector]
    public NavMeshAgent agent = null!;
    [HideInInspector]
    public Coroutine? checkPathsRoutine = null;

    [Header("Search Algorithm")]
    [SerializeField]
    private float _nodeRemovalPrecision = 5f;
    [SerializeField]
    private SmartPathfindingLinkFlags _allowedLinks = SmartPathfindingLinkFlags.InternalTeleports | SmartPathfindingLinkFlags.MainEntrance | SmartPathfindingLinkFlags.Elevators | SmartPathfindingLinkFlags.FireExits;

    public enum AgentState
    {
        NotSet,
        Inside,
        Outside
    }

    public enum GoToDestinationResult
    {
        Success,
        InProgress,
        Failure,
    }

    public void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        SmartPathfinding.RegisterSmartAgent(agent);
    }

    public bool IsAgentOutside()
    {
        if (_agentState == AgentState.NotSet)
        {
            CodeRebirthLibPlugin.Logger.LogError($"{this.name} is not initialized yet! Please call `SetAllValues` first.");
            return false;
        }
        return _agentState == AgentState.Outside;
    }

    public void SetAllValues(bool isOutside)
    {
        _agentState = isOutside ? AgentState.Outside : AgentState.Inside;
    }

    private SmartPathfindingLinkFlags GetAllowedPathLinks()
    {
        return _allowedLinks;
    }

    private void UseTeleport(EntranceTeleport teleport)
    {
        if (teleport.exitPoint == null || !teleport.FindExitPoint())
            return;

        agent.Warp(teleport.exitPoint.position);
        SetThingOutsideServerRpc(new NetworkBehaviourReference(teleport));
    }

    public bool DoPathingToDestination(Vector3 destination)
    {
        if (searchRoutine != null)
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
        GoToDestinationResult result = GoToDestination(destination);
        if (result == GoToDestinationResult.Failure)
        {
            if (DetermineIfNeedToDisableAgent(destination))
            {
                return false;
            }
        }
        return result == GoToDestinationResult.Success || result == GoToDestinationResult.InProgress;
    }

    private GoToDestinationResult GoToDestination(Vector3 targetPosition)
    {
        var result = GoToDestinationResult.InProgress;

        if (pathingTask == null)
        {
            pathingTask = new SmartPathTask();
            pathingTask.StartPathTask(this.agent, this.transform.position, targetPosition, GetAllowedPathLinks());
        }

        if (!pathingTask.IsResultReady(0))
            return result;

        if (pathingTask.GetResult(0) is SmartPathDestination destination)
        {
            result = GoToSmartPathDestination(in destination) ? GoToDestinationResult.Success : GoToDestinationResult.Failure;
        }
        else
        {
            result = GoToDestinationResult.Failure;
        }

        pathingTask.StartPathTask(this.agent, this.transform.position, targetPosition, GetAllowedPathLinks());
        return result;
    }

    public void DisposeOfTasks()
    {
        pathingTask?.Dispose();
        checkPathsTask?.Dispose();
        roamingTask?.Dispose();
    }

    #region Destination Handling
    private bool GoToSmartPathDestination(in SmartPathDestination destination)
    {
        switch (destination.Type)
        {
            case SmartDestinationType.DirectToDestination:
                HandleDirectDestination(destination);
                break;
            case SmartDestinationType.InternalTeleport:
                HandleInternalTeleportDestination(destination);
                break;
            case SmartDestinationType.EntranceTeleport:
                HandleEntranceTeleportDestination(destination);
                break;
            case SmartDestinationType.Elevator:
                HandleElevatorDestination(destination);
                break;
            default:
                return false;
        }
        return true;
    }

    private void HandleDirectDestination(SmartPathDestination destination)
    {
        agent.SetDestination(destination.Position);
    }

    private void HandleInternalTeleportDestination(SmartPathDestination destination)
    {
        agent.SetDestination(destination.Position);

        if (Vector3.Distance(this.transform.position, destination.Position) >= 1f + agent.stoppingDistance)
            return;

        agent.Warp(destination.InternalTeleport!.Destination.position);
    }

    private void HandleElevatorDestination(SmartPathDestination destination)
    {
        agent.SetDestination(destination.Position);

        if (Vector3.Distance(this.transform.position, destination.Position) >= 1f + agent.stoppingDistance)
            return;

        destination.ElevatorFloor!.CallElevator();
    }

    private void HandleEntranceTeleportDestination(SmartPathDestination destination)
    {
        agent.SetDestination(destination.Position);

        if (Vector3.Distance(this.transform.position, destination.Position) >= 1f + agent.stoppingDistance)
            return;

        UseTeleport(destination.EntranceTeleport!);
    }
    #endregion

    public bool CheckPathsOngoing()
    {
        return checkPathsRoutine != null;
    }

    public void CheckPaths<T>(IEnumerable<(T, Vector3)> points, Action<List<GenericPath<T>>> action)
    {
        if (checkPathsRoutine != null)
        {
            StopCoroutine(checkPathsRoutine);
        }
        checkPathsRoutine = StartCoroutine(CheckPathsCoroutine(points, action));
    }

    private IEnumerator CheckPathsCoroutine<T>(IEnumerable<(T, Vector3)> points, Action<List<GenericPath<T>>> action)
    {
        var TList = new List<GenericPath<T>>();
        checkPathsTask ??= new SmartPathTask();
        List<Vector3> pointsVectorList = points.Select(x => x.Item2).ToList();
        List<T> pointsTList = points.Select(x => x.Item1).ToList();
        checkPathsTask.StartPathTask(this.agent, this.transform.position, pointsVectorList, GetAllowedPathLinks());
        int listSize = pointsVectorList.Count;
        Debuggers.ReplaceThis?.Log($"Checking paths for {listSize} objects");
        yield return new WaitUntil(() => checkPathsTask.IsComplete);
        for (int i = 0; i < listSize; i++)
        {
            if (!checkPathsTask.IsResultReady(i))
            {
                CodeRebirthLibPlugin.Logger.LogError($"Result for task index: {i} on {this.gameObject.name} is not ready");
                continue;
            }
            Debuggers.ReplaceThis?.Log($"Checking result for task index: {i}, is result ready: {checkPathsTask.IsResultReady(i)}, result: {checkPathsTask.GetResult(i)}");
            if (checkPathsTask.GetResult(i) is not SmartPathDestination destination)
                continue;

            TList.Add(new GenericPath<T>(pointsTList[i], checkPathsTask.GetPathLength(i)));
        }

        action(TList);
        checkPathsRoutine = null;
    }

    public void StopAgent()
    {
        if (agent.enabled && agent.isOnNavMesh)
            agent.ResetPath();

        agent.velocity = Vector3.zero;
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
            if (pathingTask != null)
            {
                pathingTask.StartPathTask(this.agent, this.transform.position, targetPosition, GetAllowedPathLinks());
            }
            else
            {
                pathingTask = new SmartPathTask();
                pathingTask.StartPathTask(this.agent, this.transform.position, targetPosition, GetAllowedPathLinks());
            }
            return;
        }

        // Calculate the new position in an arcing motion
        float normalizedDistance = Mathf.Clamp01(Vector3.Distance(transform.position, targetPosition) / distanceToTarget);
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 10f);
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
            Debuggers.ReplaceThis?.Log($"Pathing to initial destination {destination} failed, going to fallback position {hit.position} instead.");
            return true;
        }

        return false;
    }

    public float CanPathToPoint(Vector3 startPos, Vector3 endPos)
    {
        NavMeshPath path = new();
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
        // Debuggers.ReplaceThis?.Log($"[{this.gameObject.name}] Path distance: {pathDistance}");

        return pathDistance;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetThingOutsideServerRpc(NetworkBehaviourReference entranceTeleportReference)
    {
        SetThingOutsideClientRpc(entranceTeleportReference);
    }

    [ClientRpc]
    public void SetThingOutsideClientRpc(NetworkBehaviourReference entranceTeleportReference)
    {
        lastUsedEntranceTeleport = (EntranceTeleport)entranceTeleportReference;
        _agentState = !lastUsedEntranceTeleport.isEntranceToBuilding ? AgentState.Outside : AgentState.Inside;
        OnUseEntranceTeleport.Invoke(!lastUsedEntranceTeleport.isEntranceToBuilding);
    }

    private Vector3 FindClosestValidPoint()
    {
        return agent.pathEndPosition;
    }

    public void AdjustSpeedBasedOnDistance(float minDistance, float maxDistance, float minSpeed, float maxSpeed, float multiplierBoost)
    {
        float clampedDistance = Mathf.Clamp(agent.remainingDistance, minDistance, maxDistance);
        float normalizedDistance = (clampedDistance - minDistance) / (maxDistance - minDistance);

        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, normalizedDistance) * multiplierBoost;
    }

    public bool CurrentPathIsValid()
    {
        if (agent.path.status == NavMeshPathStatus.PathPartial || agent.path.status == NavMeshPathStatus.PathInvalid)
        {
            return false;
        }
        return true;
    }

    #region Search Algorithm
    public void StartSearchRoutine(float radius)
    {
        if (!agent.enabled)
            return;

        _searchRadius = radius;
        StopSearchRoutine();
        _searchRoutine = StartCoroutine(SearchAlgorithm(radius));
    }

    public void StopSearchRoutine()
    {
        if (_searchRoutine != null)
        {
            StopCoroutine(_searchRoutine);
        }
        StopAgent();
        _searchRoutine = null;
    }

    private Coroutine? _searchRoutine;
    private float _searchRadius = 50f;
    private readonly List<Vector3> _positionsToSearch = new();
    private readonly List<Vector3> _roamingPointsVectorList = new();

    private IEnumerator SearchAlgorithm(float radius)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));
        Debuggers.ReplaceThis?.Log($"Starting search routine for {this.gameObject.name} at {this.transform.position} with radius {radius}");
        _positionsToSearch.Clear();
        yield return StartCoroutine(GetSetOfAcceptableNodesForRoaming(radius));
        while (true)
        {
            Vector3 positionToTravel = _positionsToSearch.FirstOrDefault();
            if (_positionsToSearch.Count == 0 || positionToTravel == Vector3.zero)
            {
                StartSearchRoutine(radius);
                yield break;
            }
            _positionsToSearch.RemoveAt(0);
            yield return StartCoroutine(ClearProximityNodes(_positionsToSearch, positionToTravel, _nodeRemovalPrecision));
            bool reachedDestination = false;
            while (!reachedDestination)
            {
                Debuggers.ReplaceThis?.Log($"{this.gameObject.name} Search: {positionToTravel}");
                GoToDestination(positionToTravel);
                yield return new WaitForSeconds(0.5f);

                if (!agent.enabled || Vector3.Distance(this.transform.position, positionToTravel) <= 3 + agent.stoppingDistance)
                {
                    reachedDestination = true;
                }
            }
        }
    }

    private IEnumerator GetSetOfAcceptableNodesForRoaming(float radius)
    {
        _roamingPointsVectorList.Clear();

        if (_allowedLinks.HasFlag(SmartPathfindingLinkFlags.FireExits) || _allowedLinks.HasFlag(SmartPathfindingLinkFlags.MainEntrance))
        {
            if (RoundManager.Instance.insideAINodes != null) _roamingPointsVectorList.AddRange(RoundManager.Instance.insideAINodes.Where(x => x != null).Select(x => x.transform.position));
            if (RoundManager.Instance.outsideAINodes != null) _roamingPointsVectorList.AddRange(RoundManager.Instance.outsideAINodes.Where(x => x != null).Select(x => x.transform.position));
        }
        else if (IsAgentOutside())
        {
            if (RoundManager.Instance.outsideAINodes != null) _roamingPointsVectorList.AddRange(RoundManager.Instance.outsideAINodes.Where(x => x != null).Select(x => x.transform.position));
        }
        else
        {
            if (RoundManager.Instance.insideAINodes != null) _roamingPointsVectorList.AddRange(RoundManager.Instance.insideAINodes.Where(x => x != null).Select(x => x.transform.position));
        }

        if (_roamingPointsVectorList.Count == 0)
        {
            for (int i = 0; i < 20; i++)
            {
                _roamingPointsVectorList.Add(RoundManager.Instance.GetRandomNavMeshPositionInRadius(this.transform.position, radius, default));
            }
        }
        roamingTask ??= new SmartPathTask();
        roamingTask.StartPathTask(this.agent, this.transform.position, _roamingPointsVectorList, GetAllowedPathLinks());
        int listSize = _roamingPointsVectorList.Count;
        Debuggers.ReplaceThis?.Log($"Checking paths for {listSize} objects");
        yield return new WaitUntil(() => roamingTask.IsComplete);
        for (int i = 0; i < listSize; i++)
        {
            if (!roamingTask.IsResultReady(i))
            {
                CodeRebirthLibPlugin.Logger.LogError($"Roaming task {i} is not ready");
                continue;
            }

            if (roamingTask.GetResult(i) is not SmartPathDestination destination)
                continue;

            // Debuggers.ReplaceThis?.Log($"Checking result for task index: {i}, pathLength: {roamingTask.GetPathLength(i)}, position: {destination.Position} with type: {destination.Type}");
            if (roamingTask.GetPathLength(i) > radius)
                continue;

            _positionsToSearch.Add(_roamingPointsVectorList[i]);
        }
        _positionsToSearch.Shuffle();
    }

    private IEnumerator ClearProximityNodes(List<Vector3> positionsToSearch, Vector3 positionToTravel, float radius)
    {
        int count = positionsToSearch.Count;
        if (count == 0)
            yield break;

        for (int i = count - 1; i >= 0; i--)
        {
            if (Vector3.Distance(positionsToSearch[i], positionToTravel) <= radius)
            {
                positionsToSearch.RemoveAt(i);
            }
            yield return null;
        }
    }
    #endregion
}