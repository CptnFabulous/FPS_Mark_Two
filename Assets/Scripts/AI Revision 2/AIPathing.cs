using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class AIPathing
{
    /// <summary>
    /// Calculates a path while taking hazards into account, based on how the enemy AI is set up.
    ///<para>Work in progress! Currently there are no special checks, but I want to add those in later.</para>
    /// </summary>
    /// <param name="ai"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static NavMeshPath CalculatePath(AI ai, Vector3 position, float maxDistance)
    {
        // Check if a feasible path exists.
        NavMeshPath path = new NavMeshPath();
        bool success = CanMoveToDestination(ai, position, maxDistance, out path);
        if (success == false) return null;

        // TO DO: Extra AI features
        // Check if walking along this path will result in the AI doing something stupid like walking out into an enemy firing line
        // Check if the AI is aware of that problem
        // Try generating an alternate route that meets those requirements

        return path;
    }

    static bool CanMoveToDestination(AI ai, Vector3 destination, float maxDistance, out NavMeshPath path)
    {
        // Will the AI be able to form a clear path to the target?
        // Is the point close enough to bother moving to?
        path = new NavMeshPath();
        bool success = NavMesh.CalculatePath(ai.agent.transform.position, destination, ai.agent.areaMask, path);
        if (success == false) return false;
        if (path.status != NavMeshPathStatus.PathComplete) return false;
        if (AIAction.NavMeshPathDistance(path) > maxDistance) return false;


        // TO DO: Figure out what to do if the path is partial. Should the AI just go down there anyway and figure out movement options from there?
        // Maybe the AI can have a 'wing it' versus 'play it safe and go on established routes' setting
        // If the AI wings it, maybe they should try calculating the path from the opposite end, then figure out a movement technique that connects the two.


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

    public static bool FindCover(AI ai, Vector3 positionToCheckFrom, Vector3 dangerOrigin, float distance, out AIGridPoints.GridPoint coverPoint)
    {
        // Find all points within distance
        // Check for a point where line of sight to danger origin is blocked

        NavMeshAgent agent = ai.agent;

        List<AIGridPoints.GridPoint> points = AIGridPoints.Current.GetPoints(positionToCheckFrom, 0, distance, true);
        MiscFunctions.ShuffleList(points);
        foreach (AIGridPoints.GridPoint point in points)
        {
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