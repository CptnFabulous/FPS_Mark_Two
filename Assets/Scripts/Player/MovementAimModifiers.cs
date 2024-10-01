using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAimModifiers : MonoBehaviour
{
    public AimSwayHandler swayHandler;

    [Header("Movement")]
    public MovementController movementController;
    public float multiplierWhileMoving = 2f;
    public string movementMultiplierReference = "Player Movement";

    [Header("Crouching")]
    public CrouchController crouchController;
    public float multiplierWhileCrouching = 0.5f;
    public string crouchMultiplierReference = "Player Crouching";

    MultiplierStack multipliers => swayHandler.swayMultipliers;

    private void OnEnable()
    {
        multipliers[movementMultiplierReference] = 1;
        multipliers[crouchMultiplierReference] = 1;
    }
    private void OnDisable()
    {
        multipliers[movementMultiplierReference] = 1;
        multipliers[crouchMultiplierReference] = 1;
    }
    private void Update()
    {
        // Update aim based on movement
        Vector3 velocity = movementController.controlling.LocalMovementDirection;

        float magnitude = velocity.magnitude;

        /*
        // If desired velocity magnitude is greater than zero but less than the max value, use rigidbody velocity magnitude instead
        // Roughly ensures that it doesn't count if the player is flung by an external force
        // E.g. moving with no input, or moving faster than the possible input
        if (magnitude > 0)
        {
            magnitude = Mathf.Min(magnitude, movementController.localRigidbodyVelocity.magnitude);
        }
        */


        float movementLerp = magnitude / movementController.defaultSpeed;
        float lerpedMultiplier = Mathf.LerpUnclamped(1, multiplierWhileMoving, movementLerp);

        /*
        float shiftSpeed = movementController.acceleration / movementController.defaultSpeed;
        lerpedMultiplier = Mathf.MoveTowards(multipliers[crouchMultiplierReference], lerpedMultiplier, shiftSpeed * Time.deltaTime);
        */

        multipliers[movementMultiplierReference] = lerpedMultiplier;






        // Update aim based on crouching
        multipliers[crouchMultiplierReference] = Mathf.LerpUnclamped(1, multiplierWhileCrouching, crouchController.crouchTimer);
    }
}