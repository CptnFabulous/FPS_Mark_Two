using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : Character
{
    [Header("Behaviour")]
    public StateController stateController;
    public AIStateFunction deathState;

    [Header("Sensing and reaction")]
    public FieldOfView visionCone;
    public DiegeticAudioListener hearing;

    [Header("Movement and pathing")]
    public NavMeshAgent agent;
    public PhysicsAffectedAI physicsHandler;
    public float baseMovementSpeed = 5;
    public float destinationThreshold = 1;

    [Header("Aiming and targeting")]
    public AIAim aiming;
    public AITargetManager targeting;

    [Header("Animations and feedback")]
    public Animator animator;
    public AIStatusIcon statusIcon;

    AIGunAttack _attack;

    public override Transform LookTransform => aiming.viewAxis;
    public override ICharacterLookController lookController => aiming;
    public override Vector3 aimDirection => LookTransform.forward;
    public override LayerMask lookMask => visionCone.viewDetection.mask;
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
    public Character target
    {
        get => targeting.target;
        set => targeting.target = value;
    }    
    public override WeaponHandler weaponHandler => null;

    public bool reachedDestination => agent.remainingDistance < destinationThreshold;

    protected override void Awake()
    {
        base.Awake();
        if (agent != null) agent.speed = baseMovementSpeed;
    }




    private void OnDrawGizmos()
    {
        if (agent != null)
        {
            Gizmos.color = Color.blue;
            AIAction.GizmosDrawNavMeshPath(agent.path);
        }
    }




    public override void Delete()
    {
        // Pre-emptively kills AI to ensure 'on death' events occur properly
        health.Damage(health.data.max * 999, 0, false, DamageType.DeletionByGame, null, null, Vector3.zero);
        base.Delete();
    }
    protected override void Die()
    {
        base.Die();
        stateController.SwitchToState(deathState);
        
        /*
        stateController.enabled = false;
        aiming.enabled = false;
        targeting.enabled = false;
        physicsHandler.ragdollActive = true;
        //gameObject.SetActive(false);
        */
    }

    public IEnumerator TravelToDestination(Vector3 position)
    {
        if (agent == null) yield break;
        
        Debug.DrawLine(transform.position, position, Color.cyan, 5);
        agent.SetDestination(position);


        // Set the AI to look straight forward
        aiming.LookInNeutralDirection();
        
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => reachedDestination);
    }
}
