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
public class Dodge : MovementState
{
    public float speedMultiplier = 2f;
    public float duration = 0.5f;

    [Header("Inputs")]
    public SingleInput dodgeInput;
    public SingleInput directionalInput;

    public override IEnumerator AsyncProcedure()
    {
        // Get movement direction

        // Launch player in that direction
    }
}

public class Mantle : MovementState
{

}
public class Climb : MovementState
{

}
*/
