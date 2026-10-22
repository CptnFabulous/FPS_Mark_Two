using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CptnFabulous.MiscUtility;

public class PatrolArea : AIStateFunction
{
    public AIPatrolRoute patrolRoute;
    int targetPositionIndex;
    bool reverse;

    public override IEnumerator AsyncProcedure()
    {
        // If patrol data is null, just stand in place.
        if (patrolRoute == null)
        {
            navMeshAgent.isStopped = true;
            yield break;
        }

        // TO DO: get closest point on patrol route to current position
        // Get the index and set currentPointOnRoute to that


        reverse = patrolRoute.reverse;

        // For loop of moving to each position
        while (patrolRoute != null)
        {
            yield return movement.TravelToDestination(patrolRoute.positions[targetPositionIndex].position);

            int newIndex = targetPositionIndex + 1;
        }


    }
    private void OnDisable()
    {
        navMeshAgent.isStopped = false;
    }
}


public class AIPatrolRoute : MonoBehaviour
{
    public Transform[] positions;
    public bool reverse;
    public bool endToEnd;

    private void OnDrawGizmos()
    {
        if (MiscFunctions.CurrentCameraNotMain()) return;

        DebugUtility.DrawGizmoLineSequence(positions.Length, (i) => positions[i].position, !endToEnd);
    }

}
