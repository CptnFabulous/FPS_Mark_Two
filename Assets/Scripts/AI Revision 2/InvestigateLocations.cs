using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvestigateLocations : AIStateFunction
{
    [Header("Investigating")]
    public float delayBetweenChangingTargetLocation = 10;
    public int searchTeamSize = 3;
    public float allyDistanceThreshold = 5;

    [Header("State transitions")]
    [SerializeField] List<StateFunction> statesToSwitchFrom;

    Vector3 pointToCheck;
    float waitDuration;
    StateFunction onFail;

    float priorityLevel = Mathf.NegativeInfinity;
    float lastTimeOfTargetChange = Mathf.NegativeInfinity;

    private void OnDrawGizmosSelected()
    {
        if (MiscFunctions.CurrentCameraNotMain()) return;

        if (enabled == false) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(pointToCheck, 0.5f);
    }
    private void OnDisable()
    {
        aim.LookInNeutralDirection();
    }

    public bool TrySearchForNewPosition(Vector3 pointToCheck, float waitDuration, StateFunction onFail, float priority, bool ignoreCurrentState, bool overrideCooldown)
    {
        // Check that the AI is in a state where it's actually interested in investigating noises (e.g. not in combat)
        if (!ignoreCurrentState && statesToSwitchFrom.Count > 0 && statesToSwitchFrom.Contains(root.currentStateInHierarchy) == false)
        {
            rootAI.DebugLog("Not interested in searching");
            return false;
        }
        
        // If already investigating, check if there's a valid reason to override the current investigation
        if (controller.currentState == this)
        {
            if (priority < priorityLevel)
            {
                // Ignore if the new target isn't as important
                rootAI.DebugLog("Already searching for something more important");
                return false;
            }
            else if (overrideCooldown == false && (Time.time - lastTimeOfTargetChange) < delayBetweenChangingTargetLocation)
            {
                // Ignore if current target was acquired too recently (and the new target doesn't have authority to override it)
                rootAI.DebugLog("Too soon since the AI acquired a new target");
                return false;
            }
        }

        // If enough allies are already heading towards that point, there's no need to investigate further.
        if (memory.AreEnoughAlliesInvestigating(pointToCheck, allyDistanceThreshold, searchTeamSize, out _)) return false;

        rootAI.DebugLog("Will search for position");

        this.pointToCheck = pointToCheck;
        this.waitDuration = waitDuration;
        this.onFail = onFail;
        priorityLevel = priority;
        lastTimeOfTargetChange = Time.time;

        // If AI wishes to immediately travel to destination, set destination on the same frame so that AreEnoughAlliesInvestigating() can check properly if allies are going there.
        if (waitDuration <= 0) navMeshAgent.SetDestination(pointToCheck);

        // Force refresh
        controller.SwitchToState(this, true);
        return true;
    }

    public override IEnumerator AsyncProcedure()
    {
        rootAI.DebugLog("Looking at suspicious position");
        if (waitDuration > 0)
        {
            // Look towards the target's last-known position.
            yield return aim.RotateTowardsPositionAsync(pointToCheck);
            yield return new WaitForSeconds(waitDuration);
        }

        // Go to suspicious location.
        rootAI.DebugLog($"Travelling to suspicious position");
        aim.LookInNeutralDirection();
        yield return movement.TravelToDestination(pointToCheck);

        // Look around said position
        rootAI.DebugLog($"Looking around target's last-known position");
        Vector3 startDirection = transform.forward;
        yield return aim.SweepSightlineAsync(() => startDirection, new Vector2(360, 180), 0, false);

        // If nothing is detected, switch to alternate procedure
        controller.SwitchToState(onFail);
        // Reset priority value now that there's no check state
        priorityLevel = Mathf.NegativeInfinity;
    }
}