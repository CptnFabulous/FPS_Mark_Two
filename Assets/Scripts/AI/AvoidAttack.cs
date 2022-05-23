using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class AvoidAttack : MoveToDestination
{
    AttackMessage currentAttackToAvoid;
    
    [Header("Finding position")]
    public float minDistance = 5;
    public float maxDistance = 10;
    public int numberOfChecks = 15;
    public int damageThresholdForAvoidance = 0;
    [Min(0)] public float cautionMultiplier = 1;
    public bool prioritiseCover; // Does the enemy dodge attacks, or just seek cover from them?

    public override bool ReasonToMove() => currentAttackToAvoid != null;
    public override bool PositionCompromised(Vector3 position) => currentAttackToAvoid.PositionAtRisk(AI, position, cautionMultiplier, damageThresholdForAvoidance, out int potentialDamage);
    public override bool FindPosition(out Vector3 position)
    {
        position = AI.agent.destination;

        if (currentAttackToAvoid.AtRisk(AI, cautionMultiplier, damageThresholdForAvoidance) == false)
        {
            // False alarm, continue moving to normal destination
            return true;
        }


        bool bestPositionIsCover = false;
        int smallestDamageAmount = Mathf.RoundToInt(Mathf.Infinity);
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
            if (currentAttackToAvoid.PositionAtRisk(AI, samplePosition, cautionMultiplier, damageThresholdForAvoidance, out int potentialDamage))
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

            // (If AI cares about specifically getting cover) is this position cover?
            if (prioritiseCover && bestPositionIsCover && points[i].isCover == false)
            {
                // If it isn't and a cover position has been found, disregard
                continue;
            }

            // Will this position lead to the AI taking less damage than the current best position?
            if (potentialDamage > smallestDamageAmount)
            {
                // If they take more damage, disregard.
                continue;
            }

            // If a position has already been found, check if new position is better (e.g. shorter travel distance, less damage taken)
            float newPathDistance = NavMeshPathDistance(path);
            if (newPathDistance > bestPathDistance)
            {
                // If the new position is a longer distance away, disregard
                continue;
            }

            position = samplePosition;

            bestPathDistance = newPathDistance;
            smallestDamageAmount = potentialDamage;
            bestPositionIsCover = points[i].isCover;
        }
        
        // If bestPathDistance is less than Mathf.Infinity, a valid point was found
        return bestPathDistance < Mathf.Infinity;
    }
}