using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SearchForLastTargetPosition : AIStateFunction
{
    public StateFunction onSuccess;
    public StateFunction onFail;

    IEnumerator currentCoroutine;

    private void OnEnable()
    {
        currentCoroutine = SearchCoroutine();
        StartCoroutine(currentCoroutine);
    }
    private void Update()
    {
        //navMeshAgent.destination = targetManager.target.transform.position;
        if (testThing == false)
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
        testThing = false;
    }

    bool testThing = false;

    IEnumerator SearchCoroutine()
    {
        Debug.Log($"{this}: travelling to target's last-known position");
        navMeshAgent.SetDestination(targetManager.lastKnownPosition);
        yield return new WaitUntil(() => rootAI.reachedDestination);

        Debug.Log($"{this}: looking around target's last-known position");
        testThing = true;
        yield return aim.SweepSurroundings();
        testThing = false;

        Debug.Log($"{this}: Target not at last-known position");
        SwitchToState(onFail);
    }
    
}