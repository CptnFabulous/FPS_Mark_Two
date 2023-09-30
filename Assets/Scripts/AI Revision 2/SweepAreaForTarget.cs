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

    private void OnEnable()
    {
        Debug.Log($"{rootAI}: starting search");
        
        //if (pointsToCheck == null) GetPoints();
        StartNewSearch();
    }
    void Update()
    {
        aim.LookInNeutralDirection();
        
        if (targetManager.canSeeTarget == ViewStatus.Visible)
        {
            // Target found
            Debug.Log($"{rootAI}: found target");
            SwitchToState(successState);
            pointsToCheck = null;
            return;
        }

        // Clear points the AI can see (and therefore no longer needs to check)
        pointsToCheck.RemoveAll((point) => visionCone.VisionConeCheck(point.position) == ViewStatus.Visible);
        // Also remove points that are really close to the agent 
        pointsToCheck.RemoveAll((point) => Vector3.Distance(point.position, standingPosition) < distanceForInstinctCheck);
        // If all points have been checked, switch to the fail state (since the target found check occurred earlier in the frame)
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

        if (rootAI.reachedDestination) GetNextDestination();
    }
    private void OnDrawGizmosSelected()
    {
        if (pointsToCheck != null)
        {
            foreach (AIGridPoints.GridPoint gridPoint in pointsToCheck)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(gridPoint.position, navMeshAgent.transform.up);
            }
        }
    }

    void StartNewSearch()
    {
        pointsToCheck = new List<AIGridPoints.GridPoint>(AIGridPoints.Current.gridPoints);

        // Ignore paths the AI cannot reach
        NavMeshPath path = navMeshAgent.path;
        pointsToCheck.RemoveAll((p) =>
        {
            bool canReach = navMeshAgent.CalculatePath(p.position, path) && path.status == NavMeshPathStatus.PathComplete;
            return canReach == false;
        });

        // Remove all points outside the search range
        if (maxSearchDistance > 0)
        {
            pointsToCheck.RemoveAll((point) =>
            {
                return Vector3.Distance(point.position, standingPosition) > maxSearchDistance;
            });
        }
        // Assign a new destination
        GetNextDestination();
    }
    void GetNextDestination()
    {
        if (pointsToCheck.Count <= 0) return;

        // Sort points by distance
        pointsToCheck.Sort((a, b) =>
        {
            float aDis = (a.position - standingPosition).sqrMagnitude;
            float bDis = (b.position - standingPosition).sqrMagnitude;
            return aDis.CompareTo(bDis);
        });

        AIGridPoints.GridPoint pointToTravelTo = pointsToCheck.First((p) =>
        {
            // Check if distance is not too close to the current position
            float distance = Vector3.Distance(navMeshAgent.transform.position, p.position);
            return distance > distanceForInstinctCheck;
        });
        navMeshAgent.destination = pointToTravelTo.position;
    }
}

