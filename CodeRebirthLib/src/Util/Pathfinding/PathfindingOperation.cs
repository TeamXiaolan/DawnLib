using System;
using System.Collections.Generic;
using System.Linq;
using PathfindingLib.Jobs;
using PathfindingLib.Utilities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

namespace CodeRebirthLib.Util.Pathfinding;
public class FindPathThroughTeleportsOperation : PathfindingOperation
{
    private EntranceTeleport[] entranceTeleports;
    private PooledFindPathJob[] FindDestinationJobs;
    private PooledFindPathJob? FindDirectPathToDestinationJob;
    private PooledFindPathJob[] FindEntrancePointJobs;

    public FindPathThroughTeleportsOperation(IEnumerable<EntranceTeleport> entrancePoints, Vector3 startPos, Vector3 endPos, NavMeshAgent agent)
    {
        // CodeRebirthLibPlugin.ExtendedLogging("Starting FindPathThroughTeleportsOperation");

        FindDirectPathToDestinationJob = JobPools.GetFindPathJob();
        FindDirectPathToDestinationJob.Job.Initialize(startPos, endPos, agent);
        JobHandle previousJob = FindDirectPathToDestinationJob.Job.ScheduleByRef();

        entranceTeleports = entrancePoints.ToArray();
        FindEntrancePointJobs = new PooledFindPathJob[entranceTeleports.Length];
        FindDestinationJobs = new PooledFindPathJob[entranceTeleports.Length];
        for (int i = 0; i < entranceTeleports.Length; i++)
        {
            EntranceTeleport? currentEntranceTeleport = entranceTeleports[i];
            if (currentEntranceTeleport == null) continue;
            if (currentEntranceTeleport.exitPoint == null || currentEntranceTeleport.entrancePoint == null) continue;
            PooledFindPathJob findEntrancePointJob = JobPools.GetFindPathJob();
            PooledFindPathJob findDestinationJob = JobPools.GetFindPathJob();
            findEntrancePointJob.Job.Initialize(startPos, currentEntranceTeleport.entrancePoint.position, agent);
            findDestinationJob.Job.Initialize(currentEntranceTeleport.exitPoint.position, endPos, agent);
            previousJob = findEntrancePointJob.Job.ScheduleByRef(previousJob);
            previousJob = findDestinationJob.Job.ScheduleByRef(previousJob);
            FindEntrancePointJobs[i] = findEntrancePointJob;
            FindDestinationJobs[i] = findDestinationJob;
            // CodeRebirthLibPlugin.ExtendedLogging($"Started job {startPos} -> {currentEntranceTeleport.entrancePoint.position}, {currentEntranceTeleport.exitPoint.position} -> {endPos}");
        }
    }

    public override void Dispose()
    {
        if (FindDirectPathToDestinationJob != null)
        {
            JobPools.ReleaseFindPathJob(FindDirectPathToDestinationJob);
            FindDirectPathToDestinationJob = null;
        }
        foreach (PooledFindPathJob? jobWrapper in FindEntrancePointJobs)
        {
            if (jobWrapper == null) continue;
            JobPools.ReleaseFindPathJob(jobWrapper);
        }
        foreach (PooledFindPathJob? jobWrapper in FindDestinationJobs)
        {
            if (jobWrapper == null) continue;
            JobPools.ReleaseFindPathJob(jobWrapper);
        }
        entranceTeleports = [];
        FindEntrancePointJobs = [];
        FindDestinationJobs = [];
    }

    public bool TryGetShortestPath(out bool foundPath, out float totalDistance, out EntranceTeleport? entranceTeleport)
    {
        totalDistance = -1;
        float bestDistance = float.MaxValue;
        foundPath = false;
        entranceTeleport = null;

        if (FindDirectPathToDestinationJob == null)
            return false;

        PathQueryStatus statusOfDirectPathJob = FindDirectPathToDestinationJob.Job.GetStatus().GetResult();
        if (statusOfDirectPathJob == PathQueryStatus.InProgress)
        {
            // CodeRebirthLibPlugin.ExtendedLogging("Direct path job in progress");
            return false;
        }
        if (statusOfDirectPathJob == PathQueryStatus.Success)
        {
            // CodeRebirthLibPlugin.ExtendedLogging("Direct path job success with length: " + FindDirectPathToDestinationJob.Job.GetPathLength());
            bestDistance = FindDirectPathToDestinationJob.Job.GetPathLength();
            foundPath = true;
        }
        // CodeRebirthLibPlugin.ExtendedLogging("Starting TryGetShortestPath with this many entrances: " + entranceTeleports.Length);
        for (int i = 0; i < FindEntrancePointJobs.Length; i++)
        {
            if (entranceTeleports[i] == null) continue;
            if (FindEntrancePointJobs[i] == null || FindDestinationJobs[i] == null) continue;
            PathQueryStatus statusOfEntranceJob = FindEntrancePointJobs[i].Job.GetStatus().GetResult();
            PathQueryStatus statusOfDestinationJob = FindDestinationJobs[i].Job.GetStatus().GetResult();
            // CodeRebirthLibPlugin.ExtendedLogging($"Entrance job status: {statusOfEntranceJob} and destination job status: {statusOfDestinationJob}");
            if (statusOfEntranceJob == PathQueryStatus.InProgress)
            {
                // CodeRebirthLibPlugin.ExtendedLogging($"Entrance job in progress: {i}");
                return false;
            }
            if (statusOfDestinationJob == PathQueryStatus.InProgress)
            {
                // CodeRebirthLibPlugin.ExtendedLogging($"destination job in progress: {i}");
                return false;
            }
            if (statusOfEntranceJob == PathQueryStatus.Failure)
            {
                continue;
            }
            if (statusOfDestinationJob == PathQueryStatus.Failure)
            {
                continue;
            }
            float pathLengthForEntrance = FindEntrancePointJobs[i].Job.GetPathLength();
            float pathLengthForPoint = FindDestinationJobs[i].Job.GetPathLength();
            float sum = pathLengthForPoint + pathLengthForEntrance;
            // CodeRebirthLibPlugin.ExtendedLogging($"Found combined total path for {entranceTeleports[i]} with length: {sum} with entrance length: {pathLengthForEntrance} and destination length: {pathLengthForPoint}");
            if (sum < bestDistance)
            {
                entranceTeleport = entranceTeleports[i];
                bestDistance = sum;
                foundPath = true;
            }
        }
        Dispose();
        totalDistance = bestDistance;
        // CodeRebirthLibPlugin.ExtendedLogging($"Found closest entrance teleport: {entranceTeleport} and is entrance outside: {entranceTeleport?.isEntranceToBuilding}");
        return true;
    }


    public override bool HasDisposed()
    {
        return FindDirectPathToDestinationJob == null;
    }
}

public abstract class PathfindingOperation : IDisposable
{
    public abstract void Dispose();
    public abstract bool HasDisposed();
    ~PathfindingOperation()
    {
        Dispose();
    }
}