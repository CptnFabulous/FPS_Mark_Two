using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateLocations : AIStateFunction
{
    [Header("Investigating")]
    public float delayBetweenChangingTargetLocation = 10;

    //StateFunction onSuccess;
    public StateFunction onFail;

    [Header("State transitions")]
    [SerializeField] List<StateFunction> statesToSwitchFrom;

    public Vector3 positionToCheck { get; private set; }
    public float priorityLevel { get; private set; }
    public float lastTimeOfTargetChange { get; private set; } = Mathf.NegativeInfinity;

    public void TrySearchForNewPosition(Vector3 position, float priority, bool overrideCooldown)
    {
        // Check that the AI is in a state where it's actually interested in investigating noises (e.g. not in combat)
        if (statesToSwitchFrom.Count > 0 && statesToSwitchFrom.Contains(root.currentStateInHierarchy) == false) return;
        
        // Ignore if the enemy is already engaging a known target
        if (targetManager.targetExists && targetManager.viewStatus == ViewStatus.Visible)
        {
            rootAI.DebugLog("Ignored, AI has already found the target");
            return;
        }

        // Ignore if the current target is more important/suspicious
        if (priority < priorityLevel)
        {
            rootAI.DebugLog($"Ignored, location is not a high-enough priority ({priority}, {priorityLevel})");
            return;
        }

        // Ignore if the enemy has already just acquired a new position to check (and this check does not have authority to override it)
        if (overrideCooldown == false && (Time.time - lastTimeOfTargetChange) < delayBetweenChangingTargetLocation)
        {
            //rootAI.DebugLog("Ignored, it's too soon since the AI acquired a new target");
            return;
        }

        positionToCheck = position;
        priorityLevel = priority;
        lastTimeOfTargetChange = Time.time;

        //this.onSuccess = onSuccess;
        //this.onFail = onFail;

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

    private void OnDisable()
    {
        aim.LookInNeutralDirection();
        priorityLevel = 0; // Resets priority value
    }

    public override IEnumerator AsyncProcedure()
    {
        rootAI.DebugLog("Investigation started, looking in neutral direction");
        // Have the AI look in the standard direction.
        aim.LookInNeutralDirection();

        // Go to location of sound.
        rootAI.DebugLog($"{this}: travelling to suspicious position (frame {Time.frameCount})");
        yield return rootAI.TravelToDestination(positionToCheck);

        // Look around said position
        rootAI.DebugLog($"{this}: looking around target's last-known position (frame {Time.frameCount})");
        Vector3 startDirection = transform.forward;
        yield return aim.SweepSightlineAsync(() => startDirection, new Vector2(360, 180), 0, false);

        // If nothing is detected, switch to alternate procedure
        SwitchToState(onFail);
    }
}