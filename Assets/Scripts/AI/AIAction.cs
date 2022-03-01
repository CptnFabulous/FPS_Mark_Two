using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIAction : StateMachine.State
{
    public AI AI { get; private set; }
    public Combatant CombatAI
    {
        get
        {
            return AI as Combatant;
        }
    }
    public NavMeshAgent NavMeshAgent
    {
        get
        {
            return AI.agent;
        }
    }
    public AIAim AimData
    {
        get
        {
            return AI.aiming;
        }
    }
    public override void Enter(StateMachine controller)
    {
        AI = controller.GetComponent<AI>();
    }

    public static float NavMeshPathDistance(NavMeshPath path)
    {
        float distance = 0;
        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return distance;
    }
    public static bool LineOfSightCheck(Vector3 lookOrigin, Collider[] targetColliders, LayerMask detection, float viewingPathDiameter, Collider[] exceptions)
    {
        Bounds totalBounds = MiscFunctions.CombinedBounds(targetColliders);
        Vector3 targetCentre = totalBounds.center;
        float rayDistance = Vector3.Distance(lookOrigin, targetCentre) + totalBounds.extents.magnitude;
        List<RaycastHit> results = new List<RaycastHit>(Physics.SphereCastAll(lookOrigin, viewingPathDiameter / 2, targetCentre - lookOrigin, rayDistance, detection));
        results.Sort((lhs, rhs) => lhs.distance.CompareTo(rhs.distance));
        for (int i = 0; i < results.Count; i++)
        {
            // Check results against target colliders. If one matches, line of sight is established
            for (int t = 0; t < targetColliders.Length; t++)
            {
                if (results[i].collider == targetColliders[t])
                {
                    return true;
                }
            }

            bool isException = false;
            for (int e = 0; e < exceptions.Length; e++)
            {
                if (results[i].collider == exceptions[e])
                {
                    isException = true;
                }
            }
            // If collider is not a target or an exception
            if (isException == false)
            {
                return false;
            }
        }

        return false;
    }

    public static bool LineOfSightCheck(Vector3 from, Vector3 to, float viewingPathDiameter, LayerMask detection, List<Collider> exceptions)
    {
        float rayDistance = Vector3.Distance(from, to); // Calculate distance
        // Run SpherecastAll to get all colliders in the path of the object
        List<RaycastHit> results = new List<RaycastHit>(Physics.SphereCastAll(from, viewingPathDiameter / 2, to - from, rayDistance, detection));
        // Remove all results that are mentioned in the exceptions array
        results.RemoveAll(rh => exceptions.Contains(rh.collider));
        // If the results array, minus the exception colliders, is greater than zero, then it means something is blocking line of sight
        return results.Count > 0;
    }

}

public abstract class AIMovement : AIAction
{
    public float movementSpeed = 3.5f;

    public override void Enter(StateMachine controller)
    {
        base.Enter(controller);
        NavMeshAgent.speed = movementSpeed;
    }
}