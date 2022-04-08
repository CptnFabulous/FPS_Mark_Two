using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AvoidAttack : AIMovement
{
    AttackMessage attack;
    
    [Header("Finding position")]
    public int numberOfChecks = 15;
    public int damageThresholdForAvoidance = 0;
    public float minDistance = 5;
    public float maxDistance = 10;
    public bool prioritiseCover; // Does the enemy dodge attacks, or just seek cover from them?

    [Header("Reaching destination")]
    public float destinationThreshold;
    Vector3 destination;



    public bool FindSafePosition(out Vector3 position)
    {
        position = AI.agent.destination; // Assign position output to continue moving towards prior destination, in case no valid position is found

        if (attack.AtRisk(AI, damageThresholdForAvoidance) == false)
        {
            return true; // No need to move
        }

        Bounds characterBounds = AI.health.HitboxBounds;
        Vector3 boundsDifferenceFromTransform = characterBounds.center - NavMeshAgent.transform.position; // Bounds' centre relative to agent transform
        float bestPathDistance = Mathf.Infinity;

        /*
        List<AIGridPoints.GridPoint> points = new(AIGridPoints.Current.GetSpecificNumberOfPoints(numberOfChecks, NavMeshAgent.transform.position, minDistance, maxDistance, prioritiseCover));
        
        points.Sort((a, b) =>
        {

        });
        
        points.RemoveAll((p) => attack.PositionAtRisk(AI, p.position, damageThresholdForAvoidance));
        */

        
        AIGridPoints.GridPoint[] points = AIGridPoints.Current.GetSpecificNumberOfPoints(numberOfChecks, NavMeshAgent.transform.position, minDistance, maxDistance);
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 samplePosition = points[i].position;

            // Check if position is safe. If the check returns true, it isn't.
            if (attack.PositionAtRisk(AI, samplePosition, damageThresholdForAvoidance, out int potentialDamage))
            {
                continue;
            }

            // Check if position is valid for the agent to reach
            NavMeshPath path = new NavMeshPath();
            if ((NavMesh.CalculatePath(NavMeshAgent.transform.position, samplePosition, NavMeshAgent.areaMask, path) && path.status == NavMeshPathStatus.PathComplete) == false)
            {
                continue;
            }
            

            // Somehow sort and prioritise values based on distance to a particular location, if the position is cover (if prioritiseCover is enabled), and how much damage they may take


            // If a position has already been found, check if new position is better (e.g. shorter travel distance, less damage taken)
            float newPathDistance = NavMeshPathDistance(path);
            if (newPathDistance < bestPathDistance)
            {
                destination = samplePosition;
                bestPathDistance = newPathDistance;
            }
        }
        

        // If bestPathDistance is less than Mathf.Infinity, a valid point was found
        return bestPathDistance < Mathf.Infinity;
    }


    




    public override void Enter()
    {
        base.Enter();
        FindSafePosition(out destination);
    }

    public override void Loop()
    {
        NavMeshAgent.destination = destination;
        if (attack.PositionAtRisk(AI, destination, damageThresholdForAvoidance, out int potentialDamage)) // If position is compromised
        {

        }
    }

    public System.Func<bool> NoValidLocationFound() => () =>
    {
        return false;
    };
    public System.Func<bool> DestinationReached() => () =>
    {
        return NavMeshAgent.remainingDistance < destinationThreshold;
    };
}


