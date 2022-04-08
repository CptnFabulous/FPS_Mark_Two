using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class AvoidAttack : MoveToDestination
{
    AttackMessage attack;
    
    [Header("Finding position")]
    public float minDistance = 5;
    public float maxDistance = 10;
    public int numberOfChecks = 15;
    public int damageThresholdForAvoidance = 0;
    public bool prioritiseCover; // Does the enemy dodge attacks, or just seek cover from them?

    public override bool ReasonToMove() => attack != null;
    public override bool PositionCompromised(Vector3 position) => attack.PositionAtRisk(AI, position, damageThresholdForAvoidance, out int potentialDamage);
    public override bool FindPosition(out Vector3 position)
    {
        position = AI.agent.destination;

        if (attack.AtRisk(AI, damageThresholdForAvoidance) == false)
        {
            // False alarm, continue moving to normal destination
            return true;
        }


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

            /*
            Somehow sort and prioritise values based on:
            * If the position is cover (if prioritiseCover is enabled)
            * How much damage they may take
            * Distance to a particular location
            */



            // If a position has already been found, check if new position is better (e.g. shorter travel distance, less damage taken)
            float newPathDistance = NavMeshPathDistance(path);
            if (newPathDistance > bestPathDistance)
            {
                // If the new position is a longer distance away, disregard
                continue;
            }

            position = samplePosition;

            bestPathDistance = newPathDistance;
        }
        
        // If bestPathDistance is less than Mathf.Infinity, a valid point was found
        return bestPathDistance < Mathf.Infinity;
    }
}