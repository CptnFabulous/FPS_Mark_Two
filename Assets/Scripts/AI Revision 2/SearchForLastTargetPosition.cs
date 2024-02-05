using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SearchForLastTargetPosition : AIStateFunction
{
    public float timeToWaitBeforeReacquiringTarget = 5;
    //public StateFunction onSuccess;
    public StateFunction onFail;
    public InvestigateLocations investigateState;

    IEnumerator currentCoroutine;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        currentCoroutine = SearchCoroutine();
        StartCoroutine(currentCoroutine);
    }
    private void OnDisable()
    {
        StopCoroutine(currentCoroutine);
        aim.lookingInDefaultDirection = true;
    }

    IEnumerator SearchCoroutine()
    {
        // Look towards the target's last-known position.
        //Debug.Log($"{this}: target lost, checking if outside field of view");
        yield return aim.RotateTowards(targetManager.lastHit.point);

        // The AI now knows the target is blocked by cover. Wait for several seconds
        // (in case the target has simply taken cover, or to allow the player to perform a flanking maneuver) 
        //Debug.Log($"{this}: target must be behind cover, waiting cautiously");
        yield return new WaitForSeconds(timeToWaitBeforeReacquiringTarget);

        // Go to the last known position (automatically override existing priority by making it a little bit higher than the current value)
        //Debug.Log($"{this}: cannot see target, travelling to target's last-known position (frame {Time.frameCount})");
        investigateState.TrySearchForNewPosition(targetManager.lastKnownPosition, investigateState.priorityLevel + Mathf.Epsilon, onFail);
    }
    
}