using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public static class AIAction
{
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
        for (int i = 1; i < path.corners.Length; i++)
        {
            Debug.DrawLine(path.corners[i - 1], path.corners[i], colour, time);
        }
    }
    public static void GizmosDrawNavMeshPath(NavMeshPath path)
    {
        for (int i = 1; i < path.corners.Length; i++)
        {
            Gizmos.DrawLine(path.corners[i - 1], path.corners[i]);
        }
    }

    static RaycastHit[] resultArray = new RaycastHit[8];
    static GenericComparer<RaycastHit> distanceComparer = new GenericComparer<RaycastHit>((rh) => rh.distance, false);

    public static bool RaycastWithExceptions(Vector3 origin, Vector3 direction, out RaycastHit hit, float distance, LayerMask layerMask, System.Func<RaycastHit, bool> isException, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, bool printDebugMessages = false)
    {
        if (printDebugMessages) Debug.Log("Line of sight check");

        int numberOfResults = Physics.RaycastNonAlloc(origin, direction, resultArray, distance, layerMask, queryTriggerInteraction);
        System.Array.Sort(resultArray, 0, numberOfResults, distanceComparer);
        if (printDebugMessages) Debug.Log(numberOfResults);

        for (int i = 0; i < numberOfResults; i++)
        {
            RaycastHit rh = resultArray[i];

            bool exceptionFound = isException.Invoke(rh);
            if (printDebugMessages) Debug.Log($"{i}/{numberOfResults}, {rh.collider}, {exceptionFound}");
            if (exceptionFound) continue;


            // We found a valid result
            hit = rh;
            if (printDebugMessages) Debug.Log("True");
            if (printDebugMessages) Debug.DrawRay(origin, direction, Color.green);
            return true;
        }

        // If nothing else was found, return the first exception collider (or a blank value if nothing was hit at all)
        hit = numberOfResults > 0 ? resultArray[0] : new RaycastHit();

        if (printDebugMessages) Debug.Log("True");
        if (printDebugMessages) Debug.DrawRay(origin, direction, Color.red);
        return false;
    }

    public static bool LineOfSight(Vector3 from, Vector3 to, Entity fromEntity, Entity toEntity, LayerMask detection, bool printDebugMessages = false)
    {
        return LineOfSight(from, to, detection, (rh) => fromEntity.HitOwnCollider(rh) || toEntity.HitOwnCollider(rh), printDebugMessages);
    }
    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, System.Func<RaycastHit, bool> isException, bool printDebugMessages = false)
    {
        Vector3 direction = to - from;
        return !RaycastWithExceptions(from, direction, out _, direction.magnitude, detection, isException, QueryTriggerInteraction.UseGlobal, printDebugMessages);
    }
    public static bool LineOfSightToTarget(Vector3 origin, Vector3 direction, out RaycastHit hit, float viewRange, LayerMask viewDetection, IEnumerable<Collider> targetColliders, QueryTriggerInteraction detectTriggers, System.Func<RaycastHit, bool> isException)
    {
        bool raycastHit = RaycastWithExceptions(origin, direction, out hit, viewRange, viewDetection, isException, detectTriggers);
        return raycastHit && MiscFunctions.ArrayContains(targetColliders, hit.collider);
    }
    
    public static Vector3 HypotheticalLookOrigin(AI ai, Vector3 positionToLookFrom)
    {
        Vector3 offset = ai.LookTransform.position - ai.transform.position;

        //Debug.DrawLine(ai.transform.position, positionToLookFrom, Color.cyan);
        //Debug.DrawLine(positionToLookFrom, positionToLookFrom + offset, Color.cyan);
        return positionToLookFrom + offset;
    }
}