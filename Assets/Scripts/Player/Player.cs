using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{

    [Header("Player-specific classes")]
    public UnityEngine.InputSystem.PlayerInput controls;
    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public Health health;
    public WeaponHandler weapons;
    public HeadsUpDisplay headsUpDisplay;
    public override void Delete()
    {
        health.Damage(health.data.max * 99, DamageType.DeletionByGame, null);
    }

    public void Die()
    {
        movement.enabled = false;
        weapons.enabled = false;
        movement.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        stateHandler.CurrentState = PlayerStateHandler.PlayerState.Dead;
    }
}
