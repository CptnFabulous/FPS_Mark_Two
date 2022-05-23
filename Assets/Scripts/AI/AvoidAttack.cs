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


    public override void Setup()
    {
        base.Setup();
        Notification<AttackMessage>.Receivers += CheckIncomingAttack;
    }
    void CheckIncomingAttack(AttackMessage newAttack)
    {
        // A new attack has been detected!
        // Check if the attack is dangerous enough to bother avoiding
        // Check if the AI is not in the middle of avoiding another attack
        // Check if the executor this state is attached to is actually active
        if (host.enabled && currentAttackToAvoid == null)
        {
            if (newAttack.AtRisk(AI, cautionMultiplier, damageThresholdForAvoidance))
            {
                // If so, assign the new attack
                currentAttackToAvoid = newAttack;
                Debug.Log(AI.name + " is in the path of " + newAttack + "!");
            }
            else
            {
                Debug.Log(AI.name + " thinks they're safe from " + newAttack + "!");
            }
        }
        else
        {
            Debug.Log(AI.name + " is ignoring " + newAttack + " to avoid an existing attack!");
        }
    }

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

    public override void Loop()
    {
        base.Loop();
        if (currentAttackToAvoid != null && DestinationReached()) // If AI has successfully evaded the attack, remove its reference and go back to normal behaviour
        {
            Debug.Log(AI.name + " is safe from the current attack, resuming normal behaviour");
            currentAttackToAvoid = null;
        }
    }
}