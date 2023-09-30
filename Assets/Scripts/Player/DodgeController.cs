using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DodgeController : MonoBehaviour
{
    public MovementController movement;

    public float dodgeSpeed = 10;
    public float dodgeDistance = 5;
    public float dodgeCooldown = 1;
    public UnityEvent onDodge;
    float lastTimeDodged;

    bool isGrounded => movement.isGrounded;
    Vector2 movementInput => movement.movementInput;
    Rigidbody rb => movement.rigidbody;

    void OnDodge()
    {
        // If player is standing on the ground
        // If cooldown time has elapsed
        // If player is moving in a direction
        bool cooldownElapsed = Time.time - lastTimeDodged >= dodgeCooldown;
        if (!(isGrounded && cooldownElapsed && movementInput.magnitude > 0))
        {
            return;
        }

        Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y) * dodgeDistance;
        movement = transform.rotation * movement;
        rb.AddForce(movement, ForceMode.Impulse);
        onDodge.Invoke();
        lastTimeDodged = Time.time;
    }
}