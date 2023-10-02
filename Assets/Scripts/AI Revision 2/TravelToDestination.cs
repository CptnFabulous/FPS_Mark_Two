using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    protected override void OnEnable()
    {
        base.OnEnable();

        navMeshAgent.isStopped = false;

        NavMeshPath path = GetPath();
        if (path != null) navMeshAgent.path = path;
    }


    protected abstract bool IsPathViable();
    protected abstract NavMeshPath GetPath(/*out AIGridPoints.GridPoint destination*/);


    
}
