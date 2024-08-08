using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
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


    public override Character target => null; // WIP: Override to determine whatever hostile target is closest to the player's reticle

    public override Vector3 MovementDirection => movement.movementVelocity;
    public override WeaponHandler weaponHandler => weapons;

    [Header("Player-specific classes")]
    public UnityEngine.InputSystem.PlayerInput controls;
    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public WeaponHandler weapons;
    public HeadsUpDisplay headsUpDisplay;

    public override void Delete()
    {
        health.Damage(health.data.max * 999, 0, false, DamageType.DeletionByGame, null, Vector3.zero);
    }
    protected override void Die()
    {
        base.Die();

        //movement.enabled = false;
        //weapons.enabled = false;
        movement.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        stateHandler.CurrentState = PlayerStateHandler.PlayerState.Dead;
    }
}
