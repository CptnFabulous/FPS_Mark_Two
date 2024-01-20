using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CrouchController : MonoBehaviour
{
    public MovementController movementController;
    public CapsuleCollider collider;
    public LookController lookControls;
    public UnityEvent onCrouch;
    public UnityEvent onStand;

    [Header("Input")]
    public bool toggleCrouch;
    public float crouchTransitionTime = 0.5f;

    [Header("Size")]
    public float standHeight = 2;
    public float crouchHeight = 1;
    public float headDistanceFromTop = 0.2f;

    [Header("Movement")]
    public float crouchSpeedMultiplier = 0.5f;
    public float footstepVolumeMultiplier = 0.2f;

    bool crouched;
    public float crouchTimer { get; private set; }

    public bool isCrouching
    {
        get => crouched;
        set => TryChangeCrouch(value);
    }

    /// <summary>
    /// Receives the player's input value to change if they're crouching or standing.
    /// </summary>
    /// <param name="input"></param>
    void OnCrouch(InputValue input) => isCrouching = MiscFunctions.GetToggleableInput(isCrouching, input.isPressed, toggleCrouch);
    /// <summary>
    /// Attempts to either crouch or stand back up.
    /// </summary>
    /// <param name="wantsToCrouch"></param>
    void TryChangeCrouch(bool wantsToCrouch)
    {
        // Don't do anything if the player is already in the assigned state
        if (isCrouching == wantsToCrouch) return;

        //Debug.Log(this + ": Attempting to set crouching to " + wantsToCrouch);

        // If the user wants to crouch, check if they are on solid ground.
        if (wantsToCrouch)
        {
            if (movementController.isGrounded == false) wantsToCrouch = false;
        }
        else
        {
            // If the user wants to stand up, check if there is enough space to do so.
            Vector3 centre = collider.transform.TransformPoint(collider.center);
            Vector3 direction = collider.transform.TransformDirection(collider.height / 2 * Vector3.up);
            bool ceilingInWay = Physics.Raycast(centre, direction, out RaycastHit _, direction.magnitude, movementController.collisionMask);
            
            if (ceilingInWay) wantsToCrouch = true;
        }

        // If currently sprinting, try to cancel sprint
        if (wantsToCrouch)
        {
            SprintController sprint = movementController.sprintController;
            if (sprint != null)
            {
                sprint.isSprinting = false;
                if (sprint.isSprinting == true) return;
            }
        }

        //Debug.Log(this + ": Setting crouch to " + wantsToCrouch);
        crouched = wantsToCrouch;
        //speedModifiers.Add(crouchSpeedMultiplier);
        //speedModifiers.Remove(crouchSpeedMultiplier);
        (wantsToCrouch ? onCrouch : onStand).Invoke();
    }

    private void OnEnable()
    {
        isCrouching = false;
    }
    private void OnDisable()
    {
        // Cancel crouching
        isCrouching = false;
        crouchTimer = 0;
        Update();
    }
    private void Update()
    {
        float targetValue = isCrouching ? 1 : 0;
        crouchTimer = Mathf.MoveTowards(crouchTimer, targetValue, Time.deltaTime / crouchTransitionTime);

        collider.height = Mathf.Lerp(standHeight, crouchHeight, crouchTimer);
        collider.center = Vector3.up * (collider.height / 2);
        lookControls.aimAxis.transform.localPosition = new Vector3(0, collider.height - headDistanceFromTop, 0);
    }
}