using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CptnFabulous.MiscUtility;

public static class AIPathing
{
    public static bool CanMoveToDestination(AI ai, Vector3 destination, float maxDistance, ref NavMeshPath path)
    {
        if (path == null) path = new NavMeshPath();

        NavMeshAgent agent = ai.agent;

        // Create query filter for more accurate searching
        NavMeshQueryFilter filter = new NavMeshQueryFilter();
        filter.agentTypeID = agent.agentTypeID;
        filter.areaMask = agent.areaMask;

        // Will the AI be able to form a clear path to the target?
        // Is the point close enough to bother moving to?
        bool success = NavMesh.CalculatePath(agent.transform.position, destination, filter, path);
        if (success == false) return false;
        if (path.status != NavMeshPathStatus.PathComplete) return false;
        if (AIAction.NavMeshPathDistance(path) > maxDistance) return false;


        // TO DO: Figure out what to do if the path is partial. Should the AI just go down there anyway and figure out movement options from there?
        // Maybe the AI can have a 'wing it' versus 'play it safe and go on established routes' setting
        // If the AI wings it, maybe they should try calculating the path from the opposite end, then figure out a movement technique that connects the two.

        // TO DO: Extra AI features
        // Check if walking along this path will result in the AI doing something stupid like walking out into an enemy firing line
        // Check if the AI is aware of that problem
        // Try generating an alternate route that meets those requirements

        return true;
    }

    /// <summary>
    /// Check if a position has enough space around it to accomodate an agent.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="agent"></param>
    /// <returns></returns>
    public static bool PositionIsOccupied(Vector3 position, NavMeshAgent agent)
    {
        return Physics.SphereCast(position, agent.radius, agent.transform.up, out _, agent.height, AIGridPoints.Current.environmentMask);
    }

    /// <summary>
    /// Can the AI find a point within the specified distance, where line of sight to danger origin is blocked?
    /// </summary>
    /// <param name="ai"></param>
    /// <param name="positionToCheckFrom"></param>
    /// <param name="dangerOrigin"></param>
    /// <param name="distance"></param>
    /// <param name="coverPoint"></param>
    /// <returns></returns>
    public static bool FindCover(AI ai, Vector3 positionToCheckFrom, Vector3 dangerOrigin, float distance, out AIGridPoints.GridPoint coverPoint)
    {
        // Sample grid points within check radius
        int coverPointCount = AIGridPoints.Current.GetPointsInRadius(ref AIGridPoints.sampleArray, positionToCheckFrom, 0, distance, true);
        // Shuffle to ensure randomness
        MiscFunctions.Shuffle(AIGridPoints.sampleArray, 0, coverPointCount);

        NavMeshAgent agent = ai.agent;
        for (int i = 0; i < coverPointCount; i++)
        {
            AIGridPoints.GridPoint point = AIGridPoints.sampleArray[i];

            // Recreate the character's bounds, at the cover spot being checked
            Bounds characterBounds = ai.bounds;
            characterBounds.center -= agent.transform.position;
            characterBounds.center += point.position;

            // Check the direction of the attack, and that the cover's direction is within a certain angle of it
            Vector3 attackDirection = characterBounds.center - dangerOrigin;
            foreach (Vector3 coverDirection in point.coverDirections)
            {
                float angle = Vector3.Angle(attackDirection, -coverDirection);
                if (angle <= AIGridPoints.Current.coverCheckAngleSize / 2)
                {
                    coverPoint = point;
                    return true;
                }
            }
        }

        coverPoint = new AIGridPoints.GridPoint();
        return false;
    }
    /*
    public void ReduceSpeedBasedOnStepHeight(NavMeshAgent agent, float modifier)
    {
        agent.
    }
    */
}