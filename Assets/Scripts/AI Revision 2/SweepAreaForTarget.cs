using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class SweepAreaForTarget : AIStateFunction
{
    public float maxSearchDistance = 50;
    public float positionUpdateInterval = 0.5f;
    public float distanceForInstinctCheck = 2;
    public AIStateFunction successState;
    public AIStateFunction failState;

    List<AIGridPoints.GridPoint> pointsToCheck;
    float lastTimeDestinationUpdated;

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.Log($"{rootAI}: starting search");
        //if (pointsToCheck == null) GetPoints();
        StartNewSearch();
    }
    void Update()
    {
        aim.LookInNeutralDirection();
        
        // If the AI can see their target, switch to the success state.
        if (targetManager.canSeeTarget == ViewStatus.Visible)
        {
            Debug.Log($"{rootAI}: found target");
            SwitchToState(successState);
            pointsToCheck = null;
            return;
        }

        // Clear points the AI can see (and therefore no longer needs to check)
        pointsToCheck.RemoveAll((point) => visionCone.VisionConeCheck(point.position) == ViewStatus.Visible);
        // Also remove points that are really close to the agent 
        pointsToCheck.RemoveAll((point) => Vector3.Distance(point.position, standingPosition) < distanceForInstinctCheck);
        // If all points have been checked, switch to the fail state (or start the search again if no fail state is present)
        if (pointsToCheck.Count <= 0)
        {
            Debug.Log($"{rootAI}: all points have been searched");
            if (failState != null)
            {
                pointsToCheck = null;
                SwitchToState(failState);
            }
            else // If no fail state is specified, start another search
            {
                StartNewSearch();
            }
            return;
        }

        // If the AI has reached the current destination but still has more points to check, find the next one to go to.
        if (rootAI.reachedDestination) GetNextDestination();
    }
    private void OnDrawGizmosSelected()
    {
        if (pointsToCheck == null) return;
        foreach (AIGridPoints.GridPoint gridPoint in pointsToCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(gridPoint.position, navMeshAgent.transform.up);
        }
    }

    void StartNewSearch()
    {
        // Get a copy of the cached grid points
        LevelArea areaToSearch = LevelArea.FindAreaOfPosition(targetManager.lastKnownPosition);
        Debug.Log($"{this}: area to search = " + areaToSearch);
        if (areaToSearch != null)
        {
            pointsToCheck = AIGridPoints.Current.GetGridPointsInArea(areaToSearch);
        }
        else
        {
            pointsToCheck = new List<AIGridPoints.GridPoint>(AIGridPoints.Current.gridPoints);
            // Remove all points outside the search range
            if (maxSearchDistance > 0)
            {
                pointsToCheck.RemoveAll((point) => Vector3.Distance(point.position, standingPosition) > maxSearchDistance);
            }
        }
        
        // Assign a new destination
        GetNextDestination();
    }
    void GetNextDestination()
    {
        if (pointsToCheck.Count <= 0) return;

        // Sort points by distance (in reverse order since we need to iterate backwards through the array)
        pointsToCheck.Sort((a, b) =>
        {
            float aDis = (a.position - standingPosition).sqrMagnitude;
            float bDis = (b.position - standingPosition).sqrMagnitude;
            return bDis.CompareTo(aDis);
        });

        // Ignore paths the AI cannot reach
        NavMeshPath path = navMeshAgent.path;
        for (int i = pointsToCheck.Count - 1; i >= 0; i--)
        {
            Vector3 position = pointsToCheck[i].position;
            // Check if agent can reach point, and remove the entry if they can't
            bool canReach = navMeshAgent.CalculatePath(position, path) && path.status == NavMeshPathStatus.PathComplete;
            if (canReach == false)
            {
                pointsToCheck.RemoveAt(i);
                continue;
            }
            // Check if the position is not too close to the agent's current position
            if (AIAction.NavMeshPathDistance(path) <= distanceForInstinctCheck) continue;

            // We have the next destination! Assign 'path' because it was already generated.
            navMeshAgent.path = path;
            //navMeshAgent.destination = position;
            break;
        }
    }
}

