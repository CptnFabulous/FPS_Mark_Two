using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public override Transform LookTransform => movement.upperBody;
    public override LayerMask lookMask => movement.worldViewCamera.cullingMask;
    public override LayerMask attackMask
    {
        get
        {
            // If no weapon is present, default to some other kind of layer mask
            if (weapons.CurrentWeapon == null)
            {
                // Presently player cannot attack any way other than having a weapon, so return an empty layermask
                return 0;
            }

            // Currently just returns everything. I need to implement code to get the detection of the player's current weapon
            return ~0;
        }
    }

    public override Vector3 MovementDirection => movement.movementVelocity;

    [Header("Player-specific classes")]
    public UnityEngine.InputSystem.PlayerInput controls;
    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public WeaponHandler weapons;
    public HeadsUpDisplay headsUpDisplay;
    
    public override void Die()
    {
        //movement.enabled = false;
        //weapons.enabled = false;
        movement.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        stateHandler.CurrentState = PlayerStateHandler.PlayerState.Dead;

        base.Die();
    }
}
