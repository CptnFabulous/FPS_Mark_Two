using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


// Patrol around a specified route

// Idle

// Struct for patrol route

public class Patrol : AIMovement
{
    public struct Route
    {
        public Vector3[] points;
        public float reachedThreshold;
        public bool reversed;
        public bool endToEnd;
    }


    public Route currentRoute;
    public int currentPointIndex;

    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);

        // Calculate closest point
        float bestPathLength = Mathf.Infinity;
        NavMeshPath bestPathToCheck = null;
        for (int i = 0; i < currentRoute.points.Length; i++)
        {
            bool validPath = NavMesh.CalculatePath(NavMeshAgent.transform.position, currentRoute.points[i], NavMeshAgent.areaMask, bestPathToCheck) && bestPathToCheck.status == NavMeshPathStatus.PathComplete;
            if (validPath)
            {
                float newBestPathLength = AIAction.NavMeshPathDistance(bestPathToCheck);
                if (newBestPathLength < bestPathLength)
                {
                    bestPathLength = newBestPathLength;
                    currentPointIndex = i;
                }
            }
        }


    }

    public override void Update(StateMachine controller)
    {
        if (Vector3.Distance(NavMeshAgent.transform.position, NavMeshAgent.destination) < currentRoute.reachedThreshold)
        {
            if (currentRoute.reversed)
            {
                currentPointIndex--;
            }
            else
            {
                currentPointIndex++;
            }


            //MiscFunctions.WithinArray(currentPointIndex, currentRoute.points.Length) == false

            bool reachedReverseEnd = currentPointIndex <= 0 && currentRoute.reversed;
            bool reachedEnd = currentPointIndex >= currentRoute.points.Length - 1 && currentRoute.reversed == false;

            if (reachedReverseEnd || reachedEnd && currentRoute.endToEnd)
            {
                currentRoute.reversed = !currentRoute.reversed;
            }
            else if (reachedReverseEnd)
            {
                currentPointIndex = currentRoute.points.Length - 1;
            }
            else if (reachedEnd)
            {
                currentPointIndex = 0;
            }
        }

        if (NavMeshAgent.destination != currentRoute.points[currentPointIndex])
        {
            NavMeshAgent.SetDestination(currentRoute.points[currentPointIndex]);
        }
    }

    public System.Func<bool> PatrolRouteIsValid(bool condition) => () =>
    {
        bool value = (currentRoute.points.Length > 0);
        return value == condition;
    };
}