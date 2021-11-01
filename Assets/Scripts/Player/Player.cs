using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;


public enum ButtonState
{
    Inactive,
    Pressed,
    Held,
    Released
}

public class Player : Character
{

    [Header("Player-specific classes")]
    //public PlayerInput inputManager;
    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public WeaponHandler weapons;

    private void Awake()
    {
        //inputManager = GetComponent<PlayerInput>();
        //inputManager.actions.FindAction("Move").


        movement = GetComponent<MovementController>();
        movement.controlling = this;
        stateHandler = GetComponent<PlayerStateHandler>();
        stateHandler.controlling = this;
        weapons = GetComponent<WeaponHandler>();
        weapons.controller = this;
        weapons.aimOrigin = movement.head;
    }




    /*
    public void GetVector2(InputAction.CallbackContext context, System.Action<Vector2> action)
    {
        Vector2 value = context.ReadValue<Vector2>();
        action.Invoke(value);
    }
    */
}
