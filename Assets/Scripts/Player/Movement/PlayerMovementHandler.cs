using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
    public Player controlling;
    public LookController lookControls;
    public GroundingHandler groundingHandler;
    public PhysicMaterial standingMaterial;
    public PhysicMaterial movingMaterial;

    public CapsuleCollider collider => controlling.capsuleCollider;
    public Rigidbody rigidbody => controlling.rigidbody;
    public LayerMask collisionMask => MiscFunctions.GetPhysicsLayerMask(collider.gameObject.layer);
    public bool isGrounded => groundingHandler.isGrounded;

    void Awake()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void ShiftCharacterVelocityTowards(Vector3 desired, Vector3 current, float delta, Space transformSpace)
    {
        // Swap out the player collider's material for moving versus standing still.
        // And cancel if the player doesn't want to move in a specific direction.
        bool wantsToMove = desired.sqrMagnitude > 0;
        collider.material = wantsToMove ? movingMaterial : standingMaterial;
        if (!wantsToMove) return;

        // Calculate the direction the velocity needs to shift in in order to reach the desired value (accounting for delta time)
        Vector3 accelerationVector = Vector3.MoveTowards(current, desired, delta) - current;

        // Adjust velocity in desired direction
        if (transformSpace == Space.Self)
        {
            rigidbody.AddRelativeForce(accelerationVector, ForceMode.VelocityChange);
        }
        else
        {
            rigidbody.AddForce(accelerationVector, ForceMode.VelocityChange);
        }

        //Debug.Log($"{desired}, {current}, {accelerationVector}");
    }
}
