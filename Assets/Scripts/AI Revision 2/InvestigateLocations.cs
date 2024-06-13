using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateLocations : AIStateFunction
{
    public float delayBetweenChangingTargetLocation = 10;

    //StateFunction onSuccess;
    StateFunction onFail;

    IEnumerator currentCoroutine;

    public Vector3 positionToCheck { get; private set; }
    public float priorityLevel { get; private set; }
    public float lastTimeOfTargetChange { get; private set; } = Mathf.NegativeInfinity;

    public void TrySearchForNewPosition(Vector3 position, float priority, /*StateFunction onSuccess, */StateFunction onFail)
    {
        // Ignore if the enemy is already engaging a known target
        if (targetManager.targetExists && targetManager.canSeeTarget == ViewStatus.Visible)
        {
            //Debug.Log("Ignored, AI has already found the target");
            return;
        }

        // Ignore if the current target is more important/suspicious
        if (priority < priorityLevel)
        {
            //Debug.Log($"Ignored, location is not a high-enough priority ({priority}, {priorityLevel})");
            return;
        }

        // Ignore if the enemy has already just acquired a new position to check
        if (Time.time - lastTimeOfTargetChange < delayBetweenChangingTargetLocation)
        {
            //Debug.Log("Ignored, it's too soon since the AI acquired a new target");
            return;
        }

        positionToCheck = position;
        priorityLevel = priority;
        lastTimeOfTargetChange = Time.time;

        //this.onSuccess = onSuccess;
        this.onFail = onFail;

        // Switch to this state if not already there
        if (controller.currentState != this)
        {
            controller.SwitchToState(this);
        }
        else // If already searching, re-assign the last destination
        {
            navMeshAgent.SetDestination(positionToCheck);
        }
    }

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
        priorityLevel = 0; // Resets priority value
    }
    IEnumerator SearchCoroutine()
    {
        // Go to location of sound.
        // BUG: for some reason the AI sometimes skips this phase and does not actually go to the target's last position (even though all the code plays)
        //Debug.Log($"{this}: travelling to suspicious position (frame {Time.frameCount})");
        yield return rootAI.TravelToDestination(positionToCheck);

        // Look around said position
        //Debug.Log($"{this}: looking around target's last-known position (frame {Time.frameCount})");
        yield return aim.SweepSurroundings();

        // If nothing is detected, resume normal patrol route
        aim.lookingInDefaultDirection = true;
        SwitchToState(onFail);
    }
}