using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SearchForLastTargetPosition : AIStateFunction
{
    public float timeToWaitBeforeReacquiringTarget = 5;
    public StateFunction onSuccess;
    public StateFunction onFail;

    IEnumerator currentCoroutine;
    bool notUpdatingLookAutomatically = false; // Is the AI currently updating where they're looking?

    protected override void OnEnable()
    {
        base.OnEnable();
        currentCoroutine = SearchCoroutine();
        StartCoroutine(currentCoroutine);
    }
    private void Update()
    {
        //navMeshAgent.destination = targetManager.target.transform.position;
        if (notUpdatingLookAutomatically == false)
        {
            aim.LookInNeutralDirection();
        }
        
        // If the target becomes visible again, end coroutine and switch to the success state
        if (targetManager.canSeeTarget == ViewStatus.Visible)
        {
            Debug.Log("Target found");
            SwitchToState(onSuccess);
            return; // Redundant return in case I want to put more code afterwards
        }
    }
    private void OnDisable()
    {
        StopCoroutine(currentCoroutine);
        notUpdatingLookAutomatically = false;
    }


    IEnumerator SearchCoroutine()
    {
        Debug.Log($"{this}: target lost, checking if outside field of view");
        // Look towards the target's last-known position.
        yield return aim.RotateTowards(targetManager.lastHit.point);

        // The AI now knows the target is blocked by cover. Wait for several seconds
        // (in case the target has simply taken cover, or to allow the player to perform a flanking maneuver) 
        Debug.Log($"{this}: target must be behind cover, waiting cautiously");
        yield return new WaitForSeconds(timeToWaitBeforeReacquiringTarget);

        // Seek new target position
        Debug.Log($"{this}: cannot see target, travelling to target's last-known position");
        navMeshAgent.SetDestination(targetManager.lastKnownPosition);
        notUpdatingLookAutomatically = false;
        yield return new WaitUntil(() => rootAI.reachedDestination);

        Debug.Log($"{this}: looking around target's last-known position");
        notUpdatingLookAutomatically = true;
        yield return aim.SweepSurroundings();
        notUpdatingLookAutomatically = false;

        Debug.Log($"{this}: Target not at last-known position");
        SwitchToState(onFail);
    }
    
}