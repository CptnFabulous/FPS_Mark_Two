using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SearchForTarget : AIStateFunction
{
    public StateFunction onSuccess;
    public StateFunction onFail;

    Coroutine currentCoroutine;

    private void OnEnable()
    {
        //currentCoroutine = StartCoroutine(SearchRoutine());
    }
    private void Update()
    {
        navMeshAgent.destination = targetManager.target.transform.position;

        // If the target becomes visible again, end coroutine and switch to the success state
        if (targetManager.canSeeTarget == ViewStatus.Visible)
        {
            SwitchToState(onSuccess);
            return; // Redundant return in case I want to put more code afterwards
        }
    }
    private void OnDisable()
    {
        //StopCoroutine(currentCoroutine);
    }

    IEnumerator SearchRoutine()
    {
        // Go to target's last known position
        navMeshAgent.destination = targetManager.lastKnownPosition;
        yield return new WaitUntil(() => rootAI.reachedDestination);

        // Look around
        yield return aim.SweepSurroundings();


        // If not found, predict where the target could have gone
        // How would that be done?
        // Maybe it could get a map of where the player has travelled, and follow it up to a certain point
        // Equivalent to the AI 'predicting' where the player moved
        
        // Go there and look around.
        
        // If that doesn't work, return to patrolling the area
        SwitchToState(onFail);
    }
    
}
