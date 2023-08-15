using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveToDestination : AIMovement, IPriorityAction
{
    #region IPriorityAction interface stuff
    public Action actionToRun => this;
    public Func<bool> prerequisites => CanAndShouldMove();
    #endregion

    public float destinationThreshold = 0.5f;
    Vector3 destination;
    public bool destinationAssigned { get; private set; }

    /// <summary>
    /// Finds an appropriate position for the agent to move to.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract bool FindPosition(out Vector3 position);
    /// <summary>
    /// Has the agent reached the destination specified by this function?
    /// </summary>
    /// <returns></returns>
    public bool DestinationReached()
    {
        string message = name + ", destination reached: " + destinationAssigned + ", " + (NavMeshAgent.destination == destination) + ", " + (NavMeshAgent.remainingDistance < destinationThreshold);
        Debug.Log(message);

        return destinationAssigned && NavMeshAgent.destination == destination && NavMeshAgent.remainingDistance < destinationThreshold;
    }
    /// <summary>
    /// Does the agent have a reason to move?
    /// </summary>
    /// <returns></returns>
    public abstract bool ReasonToMove();
    /// <summary>
    /// Is the specified position still practical for the agent to move to?
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public abstract bool PositionCompromised(Vector3 position);
    /// <summary>
    /// Does the agent need to move to a new position, and are they able to find a valid one?
    /// </summary>
    /// <returns></returns>
    public System.Func<bool> CanAndShouldMove()
    {
        return () =>
        {
            // Is there actually a thing they need to move because of, and if so, do they still need to move to an appropriate destination?
            if (ReasonToMove() == false || DestinationReached() == true)
            {
                // If they don't have a reason (or they did but have arrived at their destination), then they don't need to move
                return false;
            }

            // If no destination is assigned, use the agent's current destination
            Vector3 currentTargetPosition = destinationAssigned ? destination : NavMeshAgent.destination;
            
            // If no destination is assigned, or the current destination is compromised, find a new one
            bool mustFindNewPosition = destinationAssigned == false || PositionCompromised(currentTargetPosition);

            if (mustFindNewPosition)
            {
                destinationAssigned = FindPosition(out destination);
            }

            return destinationAssigned;
        };
    }



    public override void Enter()
    {
        base.Enter();
        if (destinationAssigned == false) // If CanAndShouldMove did not assign a new destination before entering, assign one now
        {
            destinationAssigned = FindPosition(out destination);
        }
    }
    public override void Loop()
    {
        CanAndShouldMove().Invoke();
        NavMeshAgent.destination = destination;
    }


    public static void DrawDebugPath(UnityEngine.AI.NavMeshPath path, Color pathColour, Color destinationColour, float duration)
    {
        for (int i = 1; i < path.corners.Length; i++)
        {
            Debug.DrawLine(path.corners[i - 1], path.corners[i], pathColour, duration);
        }
        Debug.DrawRay(path.corners[path.corners.Length - 1], Vector3.up, destinationColour, duration);
    }
}
