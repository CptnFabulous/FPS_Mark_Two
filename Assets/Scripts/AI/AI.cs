using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (showDebugData == false) return;

        Camera camera = Camera.main;
        Vector3 worldPosition = LookTransform.position + 0.5f * Vector3.up;

        if (Vector3.Dot(camera.transform.forward, worldPosition - camera.transform.position) < 0) return;

        string text = name;
        text += '\n';
        text += stateController.currentStateInHierarchy.name;
        text += '\n';
        text += aiming.currentLookMode;

        Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
        screenPosition.y = Screen.height - screenPosition.y;

        Vector2 size = new Vector2(250, 50);

        GUI.Label(new Rect(screenPosition - (size / 2), size), text, EditorStyles.centeredGreyMiniLabel);
    }
#endif

    protected override void Die()
    {
        base.Die();
        stateController.SwitchToState(deathState);
    }

    public IEnumerator TravelToDestination(Vector3 position)
    {
        if (agent == null) yield break;

        agent.isStopped = false;

        //Debug.DrawLine(transform.position, position, Color.cyan, 5);
        agent.SetDestination(position);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => reachedDestination);
    }
}
