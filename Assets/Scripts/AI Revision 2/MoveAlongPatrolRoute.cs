using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveAlongPatrolRoute : AIState
{
    /*
    private void OnDisable()
    {
        
    }
    */

    public string routeName;
    public float destinationThreshold = 0.5f;
    [HideInInspector] public bool reverse;

    PatrolRoute r;
    int index = 0;

    PatrolRoute route => r ??= GameObject.Find(routeName).GetComponent<PatrolRoute>();

    public override Status GetStatus() => (route != null) ? Status.Active : Status.Blocked;

    protected override void OnEnter()
    {
        // Get the index of the closest point on the route.
        int bestIndex = 0;
        float bestLength = Mathf.Infinity;
        NavMeshPath path = new NavMeshPath();
        for (int i = 0; i < route.points.Length; i++)
        {
            NavMesh.CalculatePath(navMeshAgent.transform.position, route.points[i].position, navMeshAgent.areaMask, path);
            float length = AIAction.NavMeshPathDistance(path);
            if (path.status == NavMeshPathStatus.PathComplete && length < bestLength)
            {
                bestIndex = i;
                bestLength = length;
            }
        }

        index = bestIndex;
    }
    public override void OnUpdate()
    {
        // Set the destination to the next point on the route
        // Once the point is reached, set the index to the next one
        navMeshAgent.SetDestination(route.points[index].position);
        if (navMeshAgent.remainingDistance < destinationThreshold)
        {
            SetNextPoint(route, ref index, ref reverse);
            Debug.Log($"{controller.gameObject}: new index is {index}, length = {route.points.Length}, frame = {Time.frameCount}");
        }
    }
    static void SetNextPoint(PatrolRoute route, ref int index, ref bool reverse)
    {
        int length = route.points.Length;
        int increment = (reverse) ? -1 : 1;

        int newIndex = index + increment;
        if (index < 0 || index >= length) // Check if the index is outside the range
        {
            // If end to end is enabled, change the reverse direction and tweak the index accordingly
            if (route.endToEnd)
            {
                reverse = !reverse;
                newIndex = index + -increment; // Reverse the increment so it goes in the other direction
            }
            else // Otherwise, just loop it
            {
                newIndex = MiscFunctions.LoopIndex(newIndex, length);
            }
        }

        index = newIndex;
    }

    /*
    public static int FindBestIndex<T>(IEnumerable<T> collection, System.Comparison<T> comparison)
    {
        FindBest(collection, comparison, out _, out int index);
        return index;
    }
    public static T FindBest<T>(IEnumerable<T> collection, System.Comparison<T> comparison)
    {
        FindBest(collection, comparison, out T best, out _);
        return best;
    }
    public static void FindBest<T>(IEnumerable<T> collection, System.Comparison<T> comparison, out T best, out int index)
    {
        best = default;
        index = 0;
        foreach (T t in collection)
        {
            if (comparison.Invoke(t, best) > 0)
            {
                best = t;
            }
            index++;
        }

        best = default;
        index = -1;
    }
    */
}
