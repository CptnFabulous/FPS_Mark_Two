using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : Character
{
    [Header("Behaviour")]
    public ActionExecutor actions;

    [Header("Movement and pathing")]
    public NavMeshAgent agent;
    public float baseMovementSpeed = 5;
    public float destinationThreshold = 1;

    [Header("Aiming and targeting")]
    public AIAim aiming;
    public AITargetManager targeting;

    public override Transform LookTransform => aiming.viewAxis;
    public override Vector3 aimDirection => LookTransform.forward;
    public override LayerMask lookMask => aiming.Stats.lookDetection;
    public override LayerMask attackMask => aiming.Stats.lookDetection;
    public override Vector3 MovementDirection => agent.velocity;
    public override Character target => targeting.target;

    public bool reachedDestination => agent.remainingDistance < destinationThreshold;

    public override void Die()
    {
        //agent.enabled = false;
        //aiming.enabled = false;
        //actions.enabled = false;

        base.Die();

        gameObject.SetActive(false);
    }
}
