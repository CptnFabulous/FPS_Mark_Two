using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDeathState : AIStateFunction
{
    public PuppetmasterRagdollHandler ragdollHandler;

    protected override void OnEnable()
    {
        // Do nothing if AI is not actually dead
        if (rootAI.health.IsAlive) return;

        rootAI.DebugLog($"Entering death state");
        base.OnEnable();

        // Ragdollise enemy
        //rootAI.aiming.enabled = false;
        //rootAI.targeting.enabled = false;
        if (rootAI.physicsHandler != null) rootAI.physicsHandler.ragdollActive = true;
    }
    private void OnDisable()
    {
        if (rootAI.physicsHandler != null) rootAI.physicsHandler.ragdollActive = false;
        //rootAI.aiming.enabled = true;
        //rootAI.targeting.enabled = true;
        if (ragdollHandler == null) return;
        ragdollHandler.ForceAllStandUpValues();
    }

    public override IEnumerator AsyncProcedure()
    {
        if (ragdollHandler == null) yield break;
        yield return ragdollHandler.CollapseAsync();

    }
    /*
    public override IEnumerator AsyncExit()
    {
        if (ragdollHandler == null) yield break;
        ragdollHandler.CheckConditionsToStandUp(out Vector3 newBasePosition, out Quaternion newBaseRotation, out _, out _, out float dot);
        yield return ragdollHandler.StandUpAsync(newBasePosition, newBaseRotation, dot);
    }
    */
}
