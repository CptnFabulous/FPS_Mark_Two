using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CptnFabulous.StateMachines;

public abstract class TravelToDestination : AIStateFunction
{
    public float destinationThreshold = 0.5f;
    public bool endStateOnceDestinationReached = true;

    //protected AIGridPoints.GridPoint destinationPoint;

    // Kinds of movement

    // Searching for target
    // Moving to ideal point to shoot target
    // Avoiding an incoming attack
    // Flanking maneuver

    public bool DestinationReached() => navMeshAgent.remainingDistance <= destinationThreshold;
    /*
    public override Status GetStatus()
    {
        NavMeshPath path = navMeshAgent.path;

        bool viable = IsPathViable();
        //Debug.Log($"{path != null}, {path.status}, {valid}");
        if (path != null && path.status == NavMeshPathStatus.PathComplete && viable)
        {
            if (endStateOnceDestinationReached && DestinationReached())
            {
                return Status.Completed; // Agent has already reached destination
            }
            return Status.Active; // Agent is moving towards destination
        }
        return Status.Blocked; // Destination is either invalid or no longer viable
    }
    */
    protected virtual void OnEnable()
    {
        navMeshAgent.isStopped = false;

        NavMeshPath path = GetPath();
        if (path != null) navMeshAgent.path = path;

    }


    protected abstract bool IsPathViable();
    protected abstract NavMeshPath GetPath(/*out AIGridPoints.GridPoint destination*/);


    
}
