using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class HorizontalDodge : MovementState
{
    [Header("Stats")]
    public float speedMultiplier = 3;
    public float duration = 0.25f;
    [Tooltip("Time range represents duration of dodge, value represents movement speed multiplier")]
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float staminaCost = 1;
    public float cooldown = 0.5f;

    [Header("Inputs")]
    public SingleInput dodgeInput;
    public SingleInput directionalInput;

    [Header("Additional")]
    public MovementController normalMovement;
    public UnityEvent onDodge;

    Quaternion originalRotation;
    Vector2 originalInput;
    float lastTimePerformed = Mathf.NegativeInfinity;

    void Awake()
    {
        directionalInput.onActionPerformed.AddListener((ctx) => originalInput = ctx.ReadValue<Vector2>());
        dodgeInput.onActionPerformed.AddListener(RegisterInput);
    }

    void RegisterInput(InputAction.CallbackContext ctx)
    {
        if (CanPerform(ctx) == false) return;

        // Preserve rotation at time dodge was performed
        // TO DO: should I update this to also take into account the angle of the ground? For when dodging up a slope?
        originalRotation = controlling.transform.rotation;

        controller.SwitchToState(this);
    }

    public override IEnumerator AsyncProcedure()
    {
        // Force the player to stand up first. If they can't do so, cancel the function.
        bool tryCancelCrouch = normalMovement.crouchController.TryChangeCrouch(false);
        if (tryCancelCrouch == false)
        {
            controller.SwitchToState(normalMovement);
            yield break;
        }


        // Get movement direction
        Vector3 localDodgeDirection = new Vector3(originalInput.x, 0, originalInput.y);
        //Vector3 dodgeDirection = originalRotation * localDodgeDirection;

        // Deplete stamina
        controlling.stamina.Deplete(staminaCost);

        onDodge.Invoke();

        
        // Calculate speed
        float speed = normalMovement.defaultSpeed * speedMultiplier;

        // Get base direction
        // Multiply by desired speed
        // Change Y axis to represent current local velocity, since the dodge isn't meant to affect existing momentum on said axis
        Vector3 desiredLocalVelocity = speed * localDodgeDirection;
        desiredLocalVelocity.y = movementHandler.localVelocity.y;
        // Launch player in that direction
        Vector3 desiredVelocity = originalRotation * desiredLocalVelocity;
        movementHandler.ShiftCharacterVelocityTowards(desiredVelocity, rigidbody.velocity, Mathf.Infinity, Space.World);

        // Wait for duration
        yield return new WaitForSeconds(duration);
        // Restore standing material so velocity can decelerate
        collider.material = movementHandler.standingMaterial;
        // Reset cooldown
        lastTimePerformed = Time.time;


        // Previous version, continuously altering velocity over curve.
        /*
        float duration = speedCurve[speedCurve.length - 1].time;
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.fixedDeltaTime;
            timer = Mathf.Min(timer, duration);

            // Calculate speed for this part of the dodge
            float speed = normalMovement.CurrentMoveSpeed * speedCurve.Evaluate(timer);

            // Get base direction
            // Multiply by desired speed
            // Change Y axis to represent current local velocity, since the dodge isn't meant to affect existing momentum on said axis
            Vector3 desiredLocalVelocity = speed * localDodgeDirection;
            desiredLocalVelocity.y = movementHandler.localVelocity.y;

            Vector3 desiredVelocity = originalRotation * desiredLocalVelocity;
            movementHandler.ShiftCharacterVelocityTowards(desiredVelocity, rigidbody.velocity, Mathf.Infinity, Space.World);

            yield return new WaitForFixedUpdate();
        }
        */

        controller.SwitchToState(normalMovement);
    }

    bool CanPerform(InputAction.CallbackContext ctx)
    {
        // Check that input was pressed and not released
        if (ctx.ReadValueAsButton() == false) return false;
        // Check if player has a movement direction in mind
        if (originalInput.sqrMagnitude <= 0) return false;
        // Check if player is grounded
        if (movementHandler.groundingHandler.isGrounded == false) return false;
        // Check if there's enough stamina
        if (controlling.stamina.values.current < staminaCost) return false;
        // Check if cooldown has not expired
        if (Time.time - lastTimePerformed < cooldown) return false;
        // Check if not already running this function
        if (controller.currentStateInHierarchy == this) return false;

        return true;
    }
}
