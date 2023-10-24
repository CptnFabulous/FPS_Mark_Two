using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolArea : AIStateFunction
{
    public AIStateFunction ifTargetFound;
    // Move around an area until the AI detects a hostile.
    // If so, switch to attack mode.

    protected override void OnEnable()
    {
        base.OnEnable();
        navMeshAgent.isStopped = true;
    }
    private void OnDisable()
    {
        navMeshAgent.isStopped = false;
    }
    private void Update()
    {
        // If a target is found, switch to the appropriate state.
        if (targetManager.targetExists)
        {
            SwitchToState(ifTargetFound);
            return;
        }

        // Look for a target
        targetManager.target = visionCone.FindObjectInFieldOfView<Character>(rootAI.IsHostileTowards, out _);
    }
}
