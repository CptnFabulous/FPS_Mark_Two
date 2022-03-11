using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EngageTargetAtDistance : AIMovement
{
    public float minimumDistance = 10;
    public float maximumDistance = 30;
    public int numberOfChecks = 15;

    [Header("Self-preservation")]
    public bool stayCloseToCover = true;
    public float maxAcceptableDistanceToCover = 3;
    public int coverChecksPerPositionCheck = 5;

    Character target
    {
        get
        {
            return CombatAI?.target;
        }
    }
    Vector3 destination;
    Vector3 nearbyCover;

    bool locationCompromised;
    bool newLocationFound;

    /// <summary>
    /// True if the location is compromised and a new location cannot be found
    /// </summary>
    /// <returns></returns>
    public System.Func<bool> UnableToEngageTarget() => ()=> locationCompromised && !newLocationFound;

    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);

        destination = AI.agent.transform.position;
        FindIdealLocation(out bool successful);
    }
    public override void Update(StateMachine controller)
    {
        locationCompromised = LocationCompromised();
        if (locationCompromised)
        {
            FindIdealLocation(out newLocationFound);
        }
        AI.agent.destination = destination;

        Debug.DrawLine(AI.transform.position, destination, Color.magenta);

        Vector3 aimOrigin = AI.RelativeLookOrigin(destination);
        Debug.DrawLine(destination, aimOrigin, Color.yellow);
        Debug.DrawLine(aimOrigin, target.CentreOfMass, Color.green);
        if (stayCloseToCover)
        {
            Vector3 centreOfMass = AI.RelativeCentreOfMass(nearbyCover);
            Debug.DrawLine(nearbyCover, centreOfMass, Color.cyan);
            Debug.DrawLine(centreOfMass, target.LookTransform.position, Color.red);
        }
    }

    public void FindIdealLocation(out bool successful)
    {
        //Vector3 checkOrigin = target.transform.position;

        float bestPathDistance = Mathf.Infinity; // Calculated once and stored so we don't have to do it every time we check against another path


        bool bestPositionIsNearCover = false;

        AIGridPoints.GridPoint[] samples = AIGridPoints.Current.GetSpecificNumberOfPoints(numberOfChecks, target.transform.position, minimumDistance, maximumDistance);
        for (int i = 0; i < samples.Length; i++)
        {
            #region Check that position is viable
            Vector3 samplePosition = samples[i].position;
            // Check if the sample is not blocked by cover, so line of sight is established
            bool lineOfSight = LineOfSight(AI.RelativeLookOrigin(samplePosition), target.CentreOfMass, AI.attackMask, AI.health.HitboxColliders, target.health.HitboxColliders);
                //LineOfSightCheck(AI.RelativeLookOrigin(samplePosition), target.health.HitboxColliders, AI.aiming.Stats.lookDetection, AI.aiming.Stats.diameterForUnobstructedSight, AI.health.HitboxColliders);
            if (lineOfSight == false)
            {
                continue;
            }

            // Check that valid path can be made
            NavMeshPath newPath = new NavMeshPath();
            bool validPath = NavMesh.CalculatePath(AI.agent.transform.position, samplePosition, NavMeshAgent.areaMask, newPath) && newPath.status == NavMeshPathStatus.PathComplete;
            if (validPath == false)
            {
                continue;
            }

            // Check if the sample is closer than the previous sample
            float newPathDistance = NavMeshPathDistance(newPath);
            if (newPathDistance > bestPathDistance)
            {
                continue;
            }
            #endregion

            // If the enemy cares about taking cover, is this position close enough to cover?
            if (stayCloseToCover)
            {
                #region Compare safety of position to that of previous best position
                bool newPositionIsNearCover = false;
                // Find valid cover within a short distance of the position
                AIGridPoints.GridPoint[] nearbyCoverPoints = AIGridPoints.Current.GetSpecificNumberOfPoints(coverChecksPerPositionCheck, samplePosition, 0, maxAcceptableDistanceToCover, true);
                for (int c = 0; c < nearbyCoverPoints.Length; c++)
                {
                    // If a cover point is safe from the player's current position (e.g. if a line of sight check fails)
                    Vector3 from = target.LookTransform.position;
                    Vector3 to = AI.RelativeCentreOfMass(nearbyCoverPoints[c].position);
                    if (LineOfSight(from, to, target.attackMask, AI.health.HitboxColliders, target.health.HitboxColliders) == false)
                    {
                        // If the line of sight check fails, the position is a safe cover point from the player
                        newPositionIsNearCover = true;
                        nearbyCover = nearbyCoverPoints[c].position;
                        break;
                    }
                }

                if (newPositionIsNearCover == false && bestPositionIsNearCover == true)
                {
                    // If best position is near cover but new position isn't, don't assign because cover is more important than distance
                    Debug.DrawRay(samplePosition, Vector3.up, Color.black, 10);
                    continue;
                }
                else
                {
                    // If both positions are near cover or not, factor is irrelevant
                    // If best position is not near cover but new position is, assign position because it's better
                    bestPositionIsNearCover = true;
                }
                #endregion
            }

            destination = samplePosition;
            bestPathDistance = newPathDistance;
        }

        // If bestPathDistance is still set to infinity, it means no path was found to have its distance recorded.
        successful = bestPathDistance < Mathf.Infinity;
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
        bool lineOfSight = LineOfSight(AI.RelativeLookOrigin(destination), target.CentreOfMass, AI.attackMask, AI.health.HitboxColliders, target.health.HitboxColliders);
        //LineOfSightCheck(AI.RelativeLookOrigin(destination), target.health.HitboxColliders, AI.aiming.Stats.lookDetection, AI.aiming.Stats.diameterForUnobstructedSight, AI.health.HitboxColliders);
        if (lineOfSight == false)
        {
            return true;
        }

        return false;
    }
}
