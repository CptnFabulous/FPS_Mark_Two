using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.InputSystem;



public class Player : Character
{

    [Header("Player-specific classes")]
    //public PlayerInput inputManager;
    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public Health health;
    public WeaponHandler weapons;
    public HeadsUpDisplay headsUpDisplay;
    /*
    private void Awake()
    {
        //inputManager = GetComponent<PlayerInput>();
        //inputManager.actions.FindAction("Move").


        movement.controlling = this;
        stateHandler.controlling = this;
        weapons.controller = this;
        headsUpDisplay.controller = this;
    }
    */



    /*
    public void GetVector2(InputAction.CallbackContext context, System.Action<Vector2> action)
    {
        Vector2 value = context.ReadValue<Vector2>();
        action.Invoke(value);
    }
    */
}
