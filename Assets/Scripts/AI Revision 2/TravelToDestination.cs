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
    protected NavMeshPath calculatedPath;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;

            bool pathFound = GetPath(ref calculatedPath);
            if (pathFound) navMeshAgent.path = calculatedPath;
        }
    }


    protected abstract bool IsPathViable();
    protected abstract bool GetPath(ref NavMeshPath path/*out AIGridPoints.GridPoint destination*/);


    
}
