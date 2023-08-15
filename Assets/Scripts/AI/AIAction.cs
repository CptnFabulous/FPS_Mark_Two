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
    public static bool LineOfSight(Vector3 from, Vector3 to, LayerMask detection, params IEnumerable<Collider>[] exceptionLists)
    {
        // Calculate direction and use magnitude for distance
        Vector3 direction = to - from;

        // Run RaycastAll to get all colliders in the path of the object
        List<RaycastHit> results = new List<RaycastHit>(Physics.RaycastAll(from, direction, direction.magnitude, detection));
        // Remove all results that are mentioned in the exceptions arrays
        
        foreach (IEnumerable<Collider> exceptions in exceptionLists)
        {
            if (exceptions == null) continue;

            foreach (Collider exception in exceptions)
            {
                results.RemoveAll(rh => rh.collider == exception);
            }
        }
        // If the results array, minus the exception colliders, is greater than zero, then it means something is blocking line of sight
        
        bool nothingBlocking = results.Count <= 0;

        Debug.DrawLine(from, to, nothingBlocking ? Color.green : Color.red);

        return nothingBlocking;
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