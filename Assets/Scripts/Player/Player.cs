using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    static List<Player> _activePlayers = new List<Player>();
    public static IReadOnlyList<Player> activePlayers => _activePlayers;
    
    public override ICharacterLookController lookController => movement.lookControls;
    public override Transform LookTransform => movement.lookControls.upperBody;
    public override Vector3 aimDirection => weapons.AimDirection;
    public override LayerMask lookMask => movement.lookControls.worldViewCamera.cullingMask;
    public override LayerMask attackMask
    {
        get
        {
            // Return the mask of whatever the player is currently holding
            if (weapons.CurrentWeapon == null)
            {
                return weapons.CurrentWeapon.CurrentMode.attackMask;
            }
            // Presently player cannot attack any way other than having a weapon, so return an empty layermask
            return 0;
        }
    }

    public override Vector3 MovementDirection => movement.movementVelocity;
    public override WeaponHandler weaponHandler => weapons;

    [Header("Player-specific classes")]
    public UnityEngine.InputSystem.PlayerInput controls;
    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public WeaponHandler weapons;
    public HeadsUpDisplay headsUpDisplay;


    private void OnEnable()
    {
        _activePlayers.Add(this);
    }
    private void OnDisable()
    {
        _activePlayers.Remove(this);
    }

    protected override void Die()
    {
        base.Die();

        SetAliveState(false);
        stateHandler.CurrentState = PlayerStateHandler.PlayerState.Dead;
    }
    public void SetAliveState(bool alive)
    {
        movement.enabled = alive;
        weapons.enabled = alive;
        rigidbody.constraints = alive ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
        if (alive)
        {
            transform.rotation = Quaternion.identity;
            //transform.rotation = Quaternion.LookRotation(LookTransform.forward, -Physics.gravity);
        }
        else
        {
            Vector3 randomEulers = new Vector3(Random.value, Random.value, Random.value);
            randomEulers = (2 * randomEulers) - Vector3.one;
            rigidbody.AddTorque(5f * randomEulers);
        }
    }

    public override bool ShouldPlayPhysicsEffects(Collision collision)
    {
        // TO DO: modify the crouch check to only matter if the collision occurred with a floor, and not a wall or ceiling
        /*
        Vector3 normal = PhysicsUtility.GetAverageCollisionNormal(collision);
        if (Vector3.Dot(normal, Physics.gravity.normalized) > -0.5f)
        {

        }
        */
        float barelyMovingThreshold = 1f;

        // Sometimes crouching or standing up retriggers a collision, even if your position relative to the ground doesn't change.
        // This can alert enemies, which is exactly what you're trying not to do when crouching.
        // So it needs to be suppressed if the player is barely moving.
        CrouchController crouchHandler = movement.crouchController;
        if (crouchHandler != null)
        {
            //if (crouchHandler.transitioning) return false;
            if (crouchHandler.transitioning && collision.relativeVelocity.magnitude < barelyMovingThreshold) return false;
        }

        return true;
    }
}
