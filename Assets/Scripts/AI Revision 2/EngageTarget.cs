using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    
    [Header("Seeking cover")]
    [SerializeField, Min(0), Tooltip("How close does the AI want to stay to cover? If set to zero, they won't bother seeking cover.")]
    float coverDistance = 2;

    [Header("Alternate states")]
    [SerializeField] AIStateFunction onTargetEliminated;
    [SerializeField] AIStateFunction onTargetLost;

    public Character target => targetManager.target;
    public float maxMoveDistance => checkDistance > 0 ? checkDistance : Mathf.Infinity;

    protected override void OnEnable()
    {
        // I might need to change this so that it only picks a new position during OnEnable() if a certain amount of time has passed between switching away from this state and returning to it.
        // So if the AI only switches for like a second (e.g. to look around) and doesn't actually need to change position, it doesn't constantly zip around like a maniac
        
        base.OnEnable();
        //currentAttack.enabled = true;
    }

    private void OnDisable()
    {
        currentAttack.enabled = false;
    }
    public void Update()
    {
        if (targetManager.targetExists == false)
        {
            SwitchToState(onTargetEliminated);
            return;
        }

        // Check if the AI can currently see the target
        bool targetIsVisible = targetManager.canSeeTarget == ViewStatus.Visible;
        currentAttack.enabled = targetIsVisible;
        if (targetIsVisible == false)
        {
            rootAI.DebugLog($"View status is {targetManager.canSeeTarget} on frame {Time.frameCount}. Hit target = {targetManager.lastHit.collider}");
            SwitchToState(onTargetLost);
            return;
        }
    }

    protected override NavMeshPath GetPath()
    {
        // Is the point within the appropriate min and max distance of the target?
        // Will the AI have appropriate line of sight to the target?
        List<AIGridPoints.GridPoint> points = AIGridPoints.Current.GetPoints(navMeshAgent.transform.position, 0, checkDistance);
        MiscFunctions.ShuffleList(points); // Shuffle points to ensure some randomisation in the final result, so the agents don't all cluster around the one ideal point

        int count = Mathf.Min(points.Count, maxNumberOfChecks);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = points[i].position;

            bool valid = VantagePointIsValid(rootAI, target, position, minTargetDistance, maxTargetDistance, coverDistance);
            if (!valid) continue;
            NavMeshPath p = AIPathing.CalculatePath(rootAI, position, maxMoveDistance);
            if (p == null) continue;

            //Debug.Log($"Selected {i + 1}/{count}");
            return p; // Return the first point that meets the criteria
        }
        //Debug.Log($"None of the {count} checked points were valid");

        //destination = new AIGridPoints.GridPoint();
        return null;
    }
    protected override bool IsPathViable()
    {
        // Check if the vantage point is still viable.
        // Unless the AI is seeking cover, in which case check the current cover position in case it's compromised.
        bool valid = VantagePointIsValid(rootAI, target, navMeshAgent.destination, minTargetDistance, maxTargetDistance, coverDistance);
        //Debug.Log($"{this}: IsPathViable() check = {valid}");
        return valid;
    }
    public static bool VantagePointIsValid(AI ai, Character target, Vector3 position, float minDistance, float maxDistance, float coverDistance)
    {
        if (ai == null) return false;
        if (target == null) return false;

        Debug.DrawRay(position, Vector3.up, Color.yellow);

        Bounds targetBounds = target.bounds;

        // Check if distance is not too close or too far
        float distance = Vector3.Distance(position, targetBounds.center);
        if (distance != Mathf.Clamp(distance, minDistance, maxDistance))
        {
            //Debug.Log($"Distance ({distance}) isn't right. Min = {minDistance}, max = {maxDistance}");
            return false;
        }

        // Check if line of sight between destination and target is not compromised
        Vector3 lookOrigin = AIAction.HypotheticalLookOrigin(ai, position);
        Debug.DrawLine(target.transform.position, targetBounds.center, Color.cyan);
        Debug.DrawLine(lookOrigin, targetBounds.center, Color.cyan);
        bool lineOfSight = AIAction.LineOfSight(lookOrigin, targetBounds.center, ai.attackMask, ai.colliders, target.colliders);
        if (lineOfSight == false)
        {
            //Debug.Log("Line of sight is broken");
            return false;
        }

        // Check if the position is close to cover
        if (coverDistance > 0)
        {
            bool foundCover = AIPathing.FindCover(ai, position, target.LookTransform.position, coverDistance, out AIGridPoints.GridPoint point);
            if (foundCover == false)
            {
                //Debug.Log("Wants cover but can't find any");
                return false;
            }
            Debug.DrawRay(point.position, Vector3.up, Color.white);
        }

        // Check if the position is occupied
        //if (AIPathing.PositionIsOccupied(position, ai.agent)) return false; 

        return true;
    }
}
