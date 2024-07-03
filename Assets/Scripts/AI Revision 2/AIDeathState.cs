using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDeathState : AIStateFunction
{
    protected override void OnEnable()
    {
        base.OnEnable();

        // Ragdollise enemy
        rootAI.aiming.enabled = false;
        rootAI.targeting.enabled = false;
        rootAI.physicsHandler.ragdollActive = true;
    }
    private void OnDisable()
    {
        rootAI.physicsHandler.ragdollActive = false;
        rootAI.aiming.enabled = true;
        rootAI.targeting.enabled = true;
    }
}
