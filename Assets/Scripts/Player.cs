using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{


    public MovementController movement;
    public PlayerStateHandler stateHandler;
    public WeaponHandler weapons;

    private void Awake()
    {
        movement = GetComponent<MovementController>();
        stateHandler = GetComponent<PlayerStateHandler>();
        weapons = GetComponent<WeaponHandler>();
    }
}
