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



    static RaycastHit[] resultArray = new RaycastHit[100];
    static List<RaycastHit> resultList = new List<RaycastHit>();

    public static bool RaycastWithExceptions(Ray ray, out RaycastHit hit, float distance, LayerMask layerMask, params IEnumerable<Collider>[] exceptionLists)
    {
        // Run RaycastAll to get all colliders in the path of the object
        resultList.Clear();
        resultList.AddRange(Physics.RaycastAll(ray, distance, layerMask));
        // Sort by distance
        MiscFunctions.SortListWithOnePredicate(resultList, (rh) => rh.distance);

        // Iterate through the options
        // Return the first result that isn't one of the exceptions
        foreach (RaycastHit rh in resultList)
        {
            if (IsExceptionCollider(rh.collider, exceptionLists)) continue;

            // We found a valid result
            hit = rh;
            return true;
        }

        // If nothing else was found, return the first exception collider (or a blank value if nothing was hit at all)
        hit = resultList.Count > 0 ? resultList[0] : new RaycastHit();
        return false;
    }

    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, params IEnumerable<Collider>[] exceptionLists)
    {
        // Run RaycastAll to get all colliders in the path of the object
        // Calculate direction and use magnitude for distance
        Vector3 direction = to - from;
        int numberOfResults = Physics.RaycastNonAlloc(from, direction, resultArray, direction.magnitude, detection);
        for (int i = 0; i < numberOfResults; i++)
        {
            // Check each collider to see if it's an exception
            RaycastHit rh = resultArray[i];
            if (IsExceptionCollider(rh.collider, exceptionLists)) continue;
            // If not, that means line of sight is blocked
            Debug.DrawLine(from, to, Color.red);
            return false;
        }

        Debug.DrawLine(from, to, Color.green);
        return true;
    }
    public static bool LineOfSightToTarget(Ray ray, out RaycastHit hit, float viewRange, LayerMask viewDetection, IEnumerable<Collider> targetColliders, QueryTriggerInteraction detectTriggers, params IEnumerable<Collider>[] exceptionLists)
    {
        int numberOfResults = Physics.RaycastNonAlloc(ray, resultArray, viewRange, viewDetection, detectTriggers);
        for (int i = 0; i < numberOfResults; i++)
        {
            hit = resultArray[i];
            // If it hit one of the desired colliders, line of sight is confirmed
            if (MiscFunctions.ArrayContains(targetColliders, hit.collider)) return true;
            // If it hit an exception, ignore and continue to the next collider
            if (IsExceptionCollider(hit.collider, exceptionLists)) continue;
            // If object hit was neither a target nor an exception, that means line of sight is blocked.
            return false;
        }

        // Target not hit at all somehow
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