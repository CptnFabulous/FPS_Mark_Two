using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementState : StateFunction
{
    public PlayerMovementHandler movementHandler;

    public Player controlling => movementHandler.controlling;
    public LookController lookControls => movementHandler.lookControls;
    public CapsuleCollider collider => movementHandler.collider;
    public Rigidbody rigidbody => movementHandler.rigidbody;
    public LayerMask collisionMask => movementHandler.collisionMask;
    public bool isGrounded => movementHandler.isGrounded;

    private void OnDisable()
    {
        collider.material = movementHandler.standingMaterial;
    }
}

/*

public class Mantle : MovementState
{

}
public class Climb : MovementState
{

}
*/
