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
    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, List<Collider> exceptions)
    {
        // Calculate direction and use magnitude for distance
        Vector3 direction = to - from;
        // Run RaycastAll to get all colliders in the path of the object
        List<RaycastHit> results = new List<RaycastHit>(Physics.RaycastAll(from, direction, direction.magnitude, detection));
        // Remove all results that are mentioned in the exceptions array
        results.RemoveAll(rh => exceptions.Contains(rh.collider));
        // If the results array, minus the exception colliders, is greater than zero, then it means something is blocking line of sight
        return results.Count <= 0;
    }
    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, Collider[] exceptions)
    {
        // Calculate direction and use magnitude for distance
        Vector3 direction = to - from;
        List<RaycastHit> results = MiscFunctions.RaycastAllWithExceptions(from, direction, direction.magnitude, detection, exceptions);
        // If the results array, minus the exception colliders, is greater than zero, then it means something is blocking line of sight
        return results.Count <= 0;
    }
    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, Collider[] exceptionsListA, Collider[] exceptionsListB)
    {
        // Calculate direction and use magnitude for distance
        Vector3 direction = to - from;
        // Run RaycastAll to get all colliders in the path of the object
        List<RaycastHit> results = new List<RaycastHit>(Physics.RaycastAll(from, direction, direction.magnitude, detection));
        // Remove all results that are mentioned in the exceptions arrays
        for (int i = 0; i < exceptionsListA.Length; i++)
        {
            results.RemoveAll(rh => rh.collider == exceptionsListA[i]);
        }
        for (int i = 0; i < exceptionsListB.Length; i++)
        {
            results.RemoveAll(rh => rh.collider == exceptionsListB[i]);
        }
        // If the results array, minus the exception colliders, is greater than zero, then it means something is blocking line of sight
        return results.Count <= 0;
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