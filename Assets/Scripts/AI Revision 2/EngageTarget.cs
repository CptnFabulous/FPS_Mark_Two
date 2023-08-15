using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EngageTarget : TravelToDestination
{
    public float checkDistance = 30;

    int maxNumberOfChecks = 100;
    //bool findNewPositionIfCompromised = true;

    [Header("Seeking target")]
    public float minTargetDistance = 5;
    public float maxTargetDistance = 15;
    /*
    [Header("Changing position")]
    float timeToSpendInPosition = 5;
    */
    [Header("Seeking cover")]
    [Min(0)]
    [Tooltip("How close does the AI want to stay to cover? If set to zero, they won't bother seeking cover.")]
    public float coverDistance = 2;

    /*
    bool currentlyAtDestination;
    float timeElapsedInCurrentPosition;
    bool seekingCover;
    */
    public Character target => rootAI.target;
    public float maxMoveDistance => checkDistance > 0 ? checkDistance : Mathf.Infinity;
    //public bool stayCloseToCover => coverDistance > 0;

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

            Debug.Log($"Selected {i + 1}/{count}");
            return p; // Return the first point that meets the criteria
        }
        Debug.Log($"None of the {count} checked points were valid");

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
    public override Status GetStatus() => Status.Active;




    


    public override void OnUpdate()
    {
        bool valid = IsPathViable();
        if (valid == false)
        {
            NavMeshPath path = GetPath();
            if (path != null)
            {
                navMeshAgent.path = GetPath();
            }
        }
        //Debug.Log($"{this}: path valid = {valid}");
        
        
        
        // If the target position becomes unviable, seek a new one
        // If the AI is behind cover, instead check the current position.

        // When the AI arrives at its destination, reset the timer.
        // Count up the timer until the time has elapsed

        // Once the time has elapsed, if stayCloseToCover is false, just select a new position.
        // If it's true, select a close-by cover position. Enable a 'seekingCover' bool so the enemy doesn't get confused. Once the position has been reached, count up time and go back out to resume 
        /*
        if (IsPathViable() == false)
        {
            navMeshAgent.path = GetPath(out destinationPoint);
        }

        // Trigger reset of time elapsed when the destination is reached
        bool reached = DestinationReached();
        if (reached != currentlyAtDestination)
        {
            Debug.Log("Agent destination reached = " + reached);
            currentlyAtDestination = reached;
            if (currentlyAtDestination)
            {
                timeElapsedInCurrentPosition = 0;
            }
        }

        if (currentlyAtDestination)
        {
            timeElapsedInCurrentPosition += Time.deltaTime;
            if (timeElapsedInCurrentPosition > timeToSpendInPosition)
            {
                Debug.Log("Seeking new position");
                navMeshAgent.path = SeekNewPosition();
            }
        }
        */


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
        bool lineOfSight = AIAction.LineOfSight(lookOrigin, targetBounds.center, ai.attackMask, ai.health.HitboxColliders, target.health.HitboxColliders);
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
