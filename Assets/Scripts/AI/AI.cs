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

    [Header("Animations and feedback")]
    public AIStatusIcon statusIcon;

    AIGunAttack _attack;

    public override Transform LookTransform => aiming.viewAxis;
    public override Vector3 aimDirection => LookTransform.forward;
    public override LayerMask lookMask => targeting.visionCone.viewDetection;
    public override LayerMask attackMask
    {
        get
        {
            // If an AIGunAttack is present, get that attack's mask.
            // If not return nothing
            // TO DO: make this system less rigid so it works with other kinds of attacks
            _attack ??= GetComponentInChildren<AIGunAttack>();
            if (_attack != null)
            {
                return _attack.weapon.attackMask;
            }

            return 0;
        }
    }
    public override Vector3 MovementDirection => agent.velocity;
    public override Character target => targeting.target;

    public bool reachedDestination => agent.remainingDistance < destinationThreshold;

    private void Awake()
    {
        agent.speed = baseMovementSpeed;
    }

    public override void Die()
    {
        //agent.enabled = false;
        //aiming.enabled = false;
        //actions.enabled = false;

        base.Die();

        gameObject.SetActive(false);
    }
}
