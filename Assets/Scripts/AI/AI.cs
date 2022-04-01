using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : Character
{
    public override Transform LookTransform => aiming.viewAxis;
    public override LayerMask lookMask => view.viewDetection;
    public override LayerMask attackMask => aiming.Stats.lookDetection;
    public override Vector3 MovementDirection => agent.velocity;

    [Header("AI-specific components")]
    public NavMeshAgent agent;
    public AIAim aiming;
    public FieldOfView view;
    public ActionExecutor actions;
    public override void Die()
    {
        agent.enabled = false;
        aiming.enabled = false;
        actions.enabled = false;
    }
}
