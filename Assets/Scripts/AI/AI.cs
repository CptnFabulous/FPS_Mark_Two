using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : Character
{
    [Header("AI-specific components")]
    public NavMeshAgent agent;
    public AIAim aiming;
    public FieldOfView view;
    public ActionExecutor actions;

    public override Transform LookTransform => aiming.viewAxis;
    public override Vector3 aimDirection => LookTransform.forward;
    public override LayerMask lookMask => aiming.Stats.lookDetection;
    public override LayerMask attackMask => aiming.Stats.lookDetection;
    public override Vector3 MovementDirection => agent.velocity;
    public override void Die()
    {
        //agent.enabled = false;
        //aiming.enabled = false;
        //actions.enabled = false;

        base.Die();

        gameObject.SetActive(false);
    }
}
