using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AvoidAttack : AIMovement
{
    AttackMessage attackToAvoid;
    
    [Header("Finding position")]
    public int numberOfChecks;
    // public int damageThresholdForDodging = 0;
    public float minDistance = 5;
    public float maxDistance = 10;
    //public bool dodgeAttacks; // Does the enemy dodge attacks, or just seek cover from them?

    [Header("Reaching destination")]
    public float destinationThreshold;
    Vector3 destination;
    

    public void FindBestPosition(AttackMessage attack, out bool successful)
    {
        // If AI is in danger and the attack damage exceeds the threshold required to bother dodging
        if ((/*attack. && */attack.AtRisk(AI)) == false)
        {
            successful = true; // No need to move
            return;
        }

        Bounds characterBounds = AI.health.HitboxBounds;
        Vector3 boundsDifferenceFromTransform = characterBounds.center - NavMeshAgent.transform.position; // Bounds' centre relative to agent transform
        float bestPathDistance = Mathf.Infinity;

        AIGridPoints.GridPoint[] points = AIGridPoints.Current.GetSpecificNumberOfPoints(numberOfChecks, NavMeshAgent.transform.position, minDistance, maxDistance);
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 samplePosition = points[i].position;
            // Update bounds centre to reflect where it would be if the agent was standing on the currently checked point
            characterBounds.center = samplePosition + boundsDifferenceFromTransform;
            // If position is dangerous, ignore
            if (attack.AtRisk(characterBounds, AI.character.health.HitboxColliders))
            {
                continue;
            }

            // If the agent cannot reach the destination, ignore
            NavMeshPath path = new NavMeshPath();
            if ((NavMesh.CalculatePath(NavMeshAgent.transform.position, samplePosition, NavMeshAgent.areaMask, path) && path.status == NavMeshPathStatus.PathComplete) == false)
            {
                continue;
            }

            float newPathDistance = NavMeshPathDistance(path);
            if (newPathDistance < bestPathDistance)
            {
                destination = samplePosition;
                bestPathDistance = newPathDistance;
            }
        }

        // If bestPathDistance is still Mathf.Infinity, it means no valid point was found
        successful = bestPathDistance < Mathf.Infinity;
    }




    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);
        FindBestPosition(attackToAvoid, out bool successful);
    }

    public override void Update(StateMachine controller)
    {
        
    }

    public override void Exit(StateMachine controller)
    {
        
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
