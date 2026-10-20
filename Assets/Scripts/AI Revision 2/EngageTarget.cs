using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CptnFabulous.MiscUtility;
using TMPro;

public class EngageTarget : TravelToDestination
{
    [Header("Attack")]
    [SerializeField] AIGunAttack currentAttack;

    [Header("Seeking target")]
    [SerializeField] float checkDistance = 30;
    int maxNumberOfChecks = 100;
    //bool findNewPositionIfCompromised = true;
    [SerializeField] float minTargetDistance = 5;
    [SerializeField] float idealTargetDistance = 10;
    [SerializeField] float maxTargetDistance = 15;
    [SerializeField, Min(0), Tooltip("How close does the AI want to stay to cover? If set to zero, they won't bother seeking cover.")]
    float distanceToCover = 3;
    [SerializeField] float distanceFromAllies = 5;

    [Header("Alternate states")]
    [SerializeField] AIStateFunction onTargetEliminated;

    [Header("On target lost")]
    public float timeAfterSightLossUntilSearch = 5;
    [SerializeField] InvestigateLocations searchForMissingTarget;
    [SerializeField] AIStateFunction onTargetLost;

    public Character target => targetManager.target;
    public float maxMoveDistance => checkDistance > 0 ? checkDistance : Mathf.Infinity;

    private void OnDisable()
    {
        currentAttack.CancelAttack();
    }
    public void Update()
    {
        // Check if enemy has been killed/deleted
        if (targetManager.targetExists == false)
        {
            controller.SwitchToState(onTargetEliminated);
            return;
        }

        // TO DO? Check how many allies are currently attacking the target.
        // If enough are, be alert but stand by in case they're killed, so AI can step in if necessary.
        // I need to figure out a good way of doing this.

        // If the AI has been unable to get line of sight on the target for an extended period, start searching more proactively.
        float timeSinceTargetLost = Time.time - targetManager.lastTimeSeenTarget;
        if (timeSinceTargetLost >= timeAfterSightLossUntilSearch)
        {
            // Check if there's data to search for the last selected position.
            Vector3 lastPosition = targetManager.lastValidHit.point;
            if (searchForMissingTarget != null && searchForMissingTarget.TrySearchForNewPosition(lastPosition, 0, onTargetLost, 1, true, true))
            {
                // If so, do nothing else this frame.
                rootAI.DebugLog($"Searching for last target position");
                return;
            }
            else
            {
                // If not, go straight to the 'on target lost' state.
                controller.SwitchToState(onTargetLost);
                return;
            }
        }

        // If path is no longer viable, select a new one
        if (navMeshAgent != null)
        {
            rootAI.showDebugData = true;
            bool destinationIsStale = VantagePointIsValid(navMeshAgent.destination) == false;
            rootAI.DebugLog($"Destination stale = {destinationIsStale}");
            rootAI.showDebugData = false;
            if (destinationIsStale)
            {
                bool pathFound = GetPath(ref calculatedPath);
                if (pathFound) navMeshAgent.SetPath(calculatedPath);
            }
        }
        
        // Run attack routines
        bool canPerformAttackRoutine = currentAttack.RunAttackRoutine();
        if (!canPerformAttackRoutine)
        {
            // If AI can't see the target, keep its sight trained on the last position.
            aim.RotateFreeLookTowards(rootAI.targeting.lastValidHit.point);
        }
    }


    static SortWithOnePredicate<AIGridPoints.GridPoint> predicate = new SortWithOnePredicate<AIGridPoints.GridPoint>();

    protected override bool GetPath(ref NavMeshPath path)
    {
        if (target == null) return false;

        Vector3 targetPosition = target.bounds.center;

        // Sample points within appropriate min and max distance
        AIGridPoints allPoints = AIGridPoints.Current;
        int numberOfPoints = allPoints.GetPointsInRadius(ref AIGridPoints.sampleArray, navMeshAgent.transform.position, 0, checkDistance);
        rootAI.DebugLog(numberOfPoints);

        // Set up sorting data, and sort array
        predicate.obtainValue = (p) =>
        {
            // For each position, get distance to target
            float distance = (targetPosition - p.position).magnitude;
            // Figure out how different that distance is from the ideal distance
            return Mathf.Abs(distance - idealTargetDistance);
        };
        predicate.reverse = false;
        System.Array.Sort(AIGridPoints.sampleArray, 0, numberOfPoints, predicate);



        // OLD METHOD: an entire list is created every time. Creates garbage.
        /*
        // Is the point within the appropriate min and max distance of the target?
        // Will the AI have appropriate line of sight to the target?
        List<AIGridPoints.GridPoint> sampledPoints = allPoints.GetPoints(navMeshAgent.transform.position, 0, checkDistance);
        Debug.Log(sampledPoints.Count);

        //CollectionUtility.ShuffleList(points); // Shuffle points to ensure some randomisation in the final result, so the agents don't all cluster around the one ideal point
        // TO DO: how can I keep results semi-ordered, but still have some randomness?
        // Maybe I can divide the results into grades, and then shuffle results within those grades?

        // Try to pick spots closer to the ideal distance
        CollectionUtility.SortListWithOnePredicate(sampledPoints, (p) =>
        {
            // For each position, get distance to target
            float distance = (targetPosition - p.position).magnitude;
            // Figure out how different that distance is from the ideal distance
            return Mathf.Abs(distance - idealTargetDistance);
        });

        int numberOfPoints = sampledPoints.Count;
        */

        // Now that we've figured out our priorities, check through each point until a valid one is found
        int count = Mathf.Min(numberOfPoints, maxNumberOfChecks);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = AIGridPoints.sampleArray[i].position;

            bool valid = VantagePointIsValid(position);
            if (!valid) continue;

            bool validPath = AIPathing.CanMoveToDestination(rootAI, position, maxMoveDistance, ref path);
            if (validPath == false) continue;

            //Debug.Log($"Selected {i + 1}/{count}");
            return true; // Return the first point that meets the criteria
        }
        
        return false;
    }
    protected override bool IsPathViable()
    {
        // Check if the vantage point is still viable.
        // Unless the AI is seeking cover, in which case check the current cover position in case it's compromised.
        // TO DO: make seeking cover be a separate state
        bool valid = VantagePointIsValid(navMeshAgent.destination);
        //Debug.Log($"{this}: IsPathViable() check = {valid}");
        return valid;
    }
    public bool VantagePointIsValid(Vector3 position)
    {
        if (rootAI == null)
        {
            rootAI.DebugLog("No valid root AI somehow");
            return false;
        }
        if (target == null)
        {
            rootAI.DebugLog("No valid target");
            return false;
        }
        //Debug.DrawRay(position, Vector3.up, Color.yellow);

        Bounds targetBounds = target.bounds;

        // Check if distance is not too close or too far
        float distanceToTarget = Vector3.Distance(position, targetBounds.center);

        if (distanceToTarget < minTargetDistance)
        {
            rootAI.DebugLog($"Distance ({distanceToTarget}) is closer than min distance {minTargetDistance}");
            return false;
        }
        if (distanceToTarget > maxTargetDistance)
        {
            rootAI.DebugLog($"Distance ({distanceToTarget}) is further than max distance {maxTargetDistance}");
            return false;
        }

        // Check if line of sight between destination and target is not compromised
        Vector3 lookOriginFromVantagePoint = AIAction.HypotheticalLookOrigin(rootAI, position);
        bool lineOfSight = currentAttack.AttackNotBlocked(lookOriginFromVantagePoint, out RaycastHit rh);
        if (lineOfSight == false)
        {
            GameObject g = rh.collider.gameObject;
            Entity entityHit = EntityCache<Entity>.GetEntity(rh.collider.gameObject);
            if (entityHit != null) g = entityHit.gameObject;

            rootAI.DebugLog($"Line of sight blocked by {g.name})");
            return false;
        }

        // Check if the position is close to cover
        if (distanceToCover > 0)
        {
            bool foundCover = AIPathing.FindCover(rootAI, position, target.attackOrigin, distanceToCover, out AIGridPoints.GridPoint point);
            if (foundCover == false)
            {
                rootAI.DebugLog("Wants cover but can't find any");
                return false;
            }
            //Debug.DrawRay(point.position, Vector3.up, Color.white);
        }

        // Check that the path isn't too close to any points that allies are moving to
        if (memory.AreEnoughAlliesInvestigating(position, distanceFromAllies, 1, out int alliesInArea))
        {
            rootAI.DebugLog($"Too many allies ({alliesInArea}) are clustered around the same vantage point");
            return false;
        }

        return true;
    }
}
