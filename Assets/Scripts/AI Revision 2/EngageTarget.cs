using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CptnFabulous.MiscUtility;

public class EngageTarget : TravelToDestination
{
    [Header("Attack")]
    [SerializeField] AIGunAttack currentAttack;

    [Header("Seeking target")]
    [SerializeField] float checkDistance = 30;
    int maxNumberOfChecks = 100;
    //bool findNewPositionIfCompromised = true;
    [SerializeField] float minTargetDistance = 5;
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
            // Check if there's data to search for the last selected position. If so, do that. 
            if (searchForMissingTarget != null)
            {
                if (searchForMissingTarget.TrySearchForNewPosition(targetManager.lastValidHit.point, 0, onTargetLost, 1, true, true))
                {
                    rootAI.DebugLog($"Target lost");
                    return;
                }
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
            if (VantagePointIsValid(navMeshAgent.destination) == false)
            {
                bool pathFound = GetPath(ref calculatedPath);
                if (pathFound) navMeshAgent.SetPath(calculatedPath);
                //navMeshAgent.SetPath(GetPath());
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

    protected override bool GetPath(ref NavMeshPath path)
    {
        // Is the point within the appropriate min and max distance of the target?
        // Will the AI have appropriate line of sight to the target?
        List<AIGridPoints.GridPoint> points = AIGridPoints.Current.GetPoints(navMeshAgent.transform.position, 0, checkDistance);
        CollectionUtility.ShuffleList(points); // Shuffle points to ensure some randomisation in the final result, so the agents don't all cluster around the one ideal point

        int count = Mathf.Min(points.Count, maxNumberOfChecks);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = points[i].position;

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
        if (rootAI == null) return false;
        if (target == null) return false;

        //Debug.DrawRay(position, Vector3.up, Color.yellow);

        Bounds targetBounds = target.bounds;

        // Check if distance is not too close or too far
        float distanceToTarget = Vector3.Distance(position, targetBounds.center);
        if (distanceToTarget != Mathf.Clamp(distanceToTarget, minTargetDistance, maxTargetDistance))
        {
            //Debug.Log($"Distance ({distance}) isn't right. Min = {minDistance}, max = {maxDistance}");
            return false;
        }

        // Check if line of sight between destination and target is not compromised
        Vector3 lookOriginFromVantagePoint = AIAction.HypotheticalLookOrigin(rootAI, position);
        bool lineOfSight = currentAttack.AttackNotBlocked(lookOriginFromVantagePoint);
        if (lineOfSight == false) return false;

        // Check if the position is close to cover
        if (distanceToCover > 0)
        {
            bool foundCover = AIPathing.FindCover(rootAI, position, target.LookTransform.position, distanceToCover, out AIGridPoints.GridPoint point);
            if (foundCover == false)
            {
                //Debug.Log("Wants cover but can't find any");
                return false;
            }
            //Debug.DrawRay(point.position, Vector3.up, Color.white);
        }

        // Check that the path isn't too close to any points that allies are moving to
        if (memory.AreEnoughAlliesInvestigating(position, distanceFromAllies, 1, out _)) return false;

        return true;
    }
}
