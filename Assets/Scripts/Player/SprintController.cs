using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

public class SprintController : MonoBehaviour
{
    public float speedMultiplier = 2;
    public float staminaPerSecond = 1;

    [Header("Utility")]
    public bool toggleInput;
    public float timeStillBeforeCancel = 0.25f;
    public MovementController movementController;
    public RegeneratingResource stamina;
    public UnityEvent onActionStart;

    bool _sprinting;
    float standStillTimer;

    public bool isSprinting
    {
        get => _sprinting;
        set
        {
            // If setting to true, check that the player can start sprinting
            if (value == true) TryStartSprint(out value);

            Debug.Log("Setting sprint to " + value);
            _sprinting = value;

            // At this point we'd change the player movement value, but that is presently handled within the MovementController class itself.

            if (value == true) onActionStart.Invoke();
        }
    }

    CrouchController crouch => movementController.crouchController;
    bool consumesStamina => staminaPerSecond > 0 && stamina != null; // Does this ability consume stamina?
    bool staminaPresent => !consumesStamina || stamina.values.current > 0; // Can always sprint if no stamina value is present (or is set to not consume stamina)
    bool isMoving => standStillTimer < timeStillBeforeCancel;
    bool canSprint => isMoving && staminaPresent;

    /// <summary>
    /// Registers the player's input to start or stop sprinting.
    /// </summary>
    /// <param name="input"></param>
    public void OnSprint(InputValue input) => isSprinting = MiscFunctions.GetToggleableInput(isSprinting, input.isPressed, toggleInput);
    void TryStartSprint(out bool willSprint)
    {
        willSprint = false;

        // Check if stamina is present and the player is currently moving.
        if (canSprint == false) return;

        // Force the player to stand up if they are crouching. If they cannot stand up, do not start sprinting.
        if (crouch != null)
        {
            crouch.isCrouching = false;
            if (crouch.isCrouching == true) return;
        }

        willSprint = true;
    }
    private void Update()
    {
        //standStillTimer = (movementController.movementInput.magnitude > 0) ? 0 : standStillTimer + Time.deltaTime;
        if (movementController.movementInput.magnitude > 0) standStillTimer = 0;
        else standStillTimer += Time.deltaTime;
        
        if (isSprinting)
        {
            // Consume stamina while sprinting
            if (consumesStamina) stamina.Deplete(staminaPerSecond * Time.deltaTime);
            // Cancel sprint if no longer able to
            if (canSprint == false) isSprinting = false;
        }
    }
}
