using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMovement : MonoBehaviour
{
    public AI rootAI;
    public float baseMovementSpeed = 3.5f;
    public float destinationThreshold = 1;

    [Header("Blinding")]
    public DetectionProfile smokeDetection;
    public float speedPenaltyInSmoke = 0.5f;

    public MultiplierStack speedMultiplierStack = new MultiplierStack();

    public NavMeshAgent navMeshAgent => rootAI.agent;
    public bool reachedDestination => navMeshAgent.remainingDistance < destinationThreshold;

    void Update()
    {
        navMeshAgent.speed = baseMovementSpeed * speedMultiplierStack.calculatedValue;

        // If enemy is moving through smoke, reduce their speed
        if (rootAI.waryOfSmoke)
        {
            Bounds bounds = rootAI.bounds;
            bool insideSmoke = Physics.CheckBox(bounds.center, 0.5f * bounds.extents, Quaternion.identity, smokeDetection.mask);
            speedMultiplierStack["Moving through smoke"] = insideSmoke ? speedPenaltyInSmoke : 1;
        }
    }

    public IEnumerator TravelToDestination(Vector3 position)
    {
        navMeshAgent.isStopped = false;

        //Debug.DrawLine(transform.position, position, Color.cyan, 5);
        navMeshAgent.SetDestination(position);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => reachedDestination);
    }
}
