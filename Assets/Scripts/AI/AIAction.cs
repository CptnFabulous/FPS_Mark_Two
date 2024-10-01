using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIAction : Action
{
    public AI AI
    {
        get
        {
            if (ai == null)
            {
                ai = host.GetComponent<AI>();
            }
            return ai;
        }
    }
    public Combatant CombatAI
    {
        get
        {
            if (cmbtnt == null)
            {
                cmbtnt = host.GetComponent<Combatant>();
            }
            return cmbtnt;
        }
    }
    public NavMeshAgent NavMeshAgent => AI.agent;
    public AIAim Aim => AI.aiming;
    AI ai;
    Combatant cmbtnt;

    

    public static float NavMeshPathDistance(NavMeshPath path)
    {
        float distance = 0;
        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return distance;
    }
    public static void DebugDrawNavMeshPath(NavMeshPath path, Color colour, float time = 0)
    {
        if (path.corners.Length <= 1) return;
        for (int i = 1; i < path.corners.Length; i++)
        {
            Debug.DrawLine(path.corners[i - 1], path.corners[i], colour, time);
        }
    }
    public static void GizmosDrawNavMeshPath(NavMeshPath path)
    {
        if (path.corners.Length <= 1) return;
        for (int i = 1; i < path.corners.Length; i++)
        {
            Gizmos.DrawLine(path.corners[i - 1], path.corners[i]);
        }
    }



    public static bool RaycastWithExceptions(Ray ray, out RaycastHit hit, float distance, LayerMask layerMask, params IEnumerable<Collider>[] exceptionLists)
    {
        // Run RaycastAll to get all colliders in the path of the object
        List<RaycastHit> results = new List<RaycastHit>(Physics.RaycastAll(ray, distance, layerMask));
        // Sort by distance
        MiscFunctions.SortListWithOnePredicate(results, (rh) => rh.distance);

        // Iterate through the options
        // Return the first result that isn't one of the exceptions
        foreach (RaycastHit rh in results)
        {
            if (IsExceptionCollider(rh.collider, exceptionLists)) continue;

            // We found a valid result
            hit = rh;
            return true;
        }

        // If nothing else was found, return the first exception collider (or a blank value if nothing was hit at all)
        hit = results.Count > 0 ? results[0] : new RaycastHit();
        return false;
    }

    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, params IEnumerable<Collider>[] exceptionLists)
    {
        // Calculate direction and use magnitude for distance
        Vector3 direction = to - from;

        // Run RaycastAll to get all colliders in the path of the object
        List<RaycastHit> results = new List<RaycastHit>(Physics.RaycastAll(from, direction, direction.magnitude, detection));
        // Remove all results that are mentioned in the exceptions arrays
        results.RemoveAll((rh) => IsExceptionCollider(rh.collider, exceptionLists));

        // If the results array, minus the exception colliders, is greater than zero, then it means something is blocking line of sight
        bool nothingBlocking = results.Count <= 0;
        Debug.DrawLine(from, to, nothingBlocking ? Color.green : Color.red);
        return nothingBlocking;
    }
    public static bool LineOfSightToTarget(Ray ray, out RaycastHit hit, float viewRange, LayerMask viewDetection, IEnumerable<Collider> targetColliders, params IEnumerable<Collider>[] exceptionLists)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, viewRange, viewDetection);
        foreach (RaycastHit rh in hits)
        {
            // If it hit one of the desired colliders, return true
            if (MiscFunctions.ArrayContains(targetColliders, rh.collider))
            {
                hit = rh;
                return true;
            }
            // If it's neither a target nor an exception, line of sight is blocked.
            if (IsExceptionCollider(rh.collider, exceptionLists) == false)
            {
                hit = rh;
                return false;
            }
            // If the collider was one of the exceptions, ignore and proceed to the next value
        }

        hit = new RaycastHit();
        return false;
    }
    static bool IsExceptionCollider(Collider toCheck, params IEnumerable<Collider>[] exceptionLists)
    {
        foreach (IEnumerable<Collider> exceptions in exceptionLists)
        {
            if (exceptions == null) continue;
            if (MiscFunctions.ArrayContains(exceptions, toCheck)) return true;
        }
        return false;
    }


    public static Vector3 HypotheticalLookOrigin(AI ai, Vector3 positionToLookFrom)
    {
        Vector3 offset = ai.LookTransform.position - ai.transform.position;

        //Debug.DrawLine(ai.transform.position, positionToLookFrom, Color.cyan);
        //Debug.DrawLine(positionToLookFrom, positionToLookFrom + offset, Color.cyan);
        return positionToLookFrom + offset;
    }
}

public abstract class AIMovement : AIAction
{
    public float movementSpeed = 3.5f;

    public override void Enter()
    {
        NavMeshAgent.speed = movementSpeed;
    }
}