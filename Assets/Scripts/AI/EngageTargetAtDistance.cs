using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EngageTargetAtDistance : AIAction
{
    public float minimumDistance = 10;
    public float maximumDistance = 30;
    public int numberOfChecks = 15;
    public LayerMask detection = ~0;

    Vector3 destination;
    
    Character target
    {
        get
        {
            return CombatAI?.target;
        }
    }
    
    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);

        destination = AI.agent.transform.position;
        FindIdealLocation(out bool successful);
    }

    public override void Update(StateMachine controller)
    {
        if (LocationCompromised())
        {
            FindIdealLocation(out bool successful);
        }
        AI.agent.destination = destination;
        Debug.DrawLine(AI.transform.position, destination, Color.magenta);
    }
    public override void Exit(StateMachine controller)
    {
        return;
    }
    



    public void FindIdealLocation(out bool successful)
    {
        Vector3 checkOrigin = target.transform.position;

        float bestPathDistance = Mathf.Infinity; // Calculated once and stored so we don't have to do it every time we check against another path

        Vector3[] samples = AIGridPoints.Current.GetSpecificNumberOfPoints(numberOfChecks, checkOrigin, minimumDistance, maximumDistance);
        for (int i = 0; i < samples.Length; i++)
        {
            // Check if the sample is not blocked by cover, so line of sight is established
            bool lineOfSight = LineOfSightCheck(NewPositionLookOrigin(samples[i]), target.health.HitboxColliders, AI.aiming.Stats.lookDetection, AI.aiming.Stats.diameterForUnobstructedSight, AI.character.health.HitboxColliders);
            if (lineOfSight == false)
            {
                continue;
            }

            // Check that valid path can be made
            NavMeshPath newPath = new NavMeshPath();
            bool validPath = NavMesh.CalculatePath(AI.agent.transform.position, samples[i], detection, newPath) && newPath.status == NavMeshPathStatus.PathComplete;
            if (validPath == false)
            {
                continue;
            }

            // Check if the sample is closer than the previous sample
            float newPathDistance = NavMeshPathDistance(newPath);
            if (newPathDistance < bestPathDistance)
            {
                destination = samples[i];
                bestPathDistance = newPathDistance;
            }
        }

        // If bestPathDistance is still set to infinity, it means no path was found to have its distance recorded.
        successful = bestPathDistance < Mathf.Infinity;
        Debug.Log("Finding new location attempt returned " + successful + ", frame " + Time.frameCount);
    }
    public bool LocationCompromised()
    {
        // Check if distance is not too close
        // Check if distance is not too far
        float distance = Vector3.Distance(destination, target.transform.position);
        if (distance < minimumDistance || distance > maximumDistance)
        {
            return true;
        }

        // Check if line of sight between destination and target is not compromised
        bool lineOfSight = LineOfSightCheck(NewPositionLookOrigin(destination), target.health.HitboxColliders, AI.aiming.Stats.lookDetection, AI.aiming.Stats.diameterForUnobstructedSight, AI.character.health.HitboxColliders);
        if (lineOfSight == false)
        {
            return true;
        }

        return false;
    }

    Vector3 NewPositionLookOrigin(Vector3 position)
    {
        Vector3 relativePosition = AI.aiming.LookOrigin - AI.transform.position;

        return position + relativePosition;
    }

    // Func<bool> that the state machine can use to determine that the location is compromised and a new location cannot be found

}