using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MovementController : MovementState
{
    [Header("Movement")]
    public SingleInput movementBinding;
    public bool canMove = true;
    public float defaultSpeed = 5;
    public float acceleration = 50;
    public CrouchController crouchController;
    public SprintController sprintController;

    [Header("Jumping")]
    public SingleInput jumpBinding;
    public float jumpForce = 5;
    public float jumpCooldown = 0.1f;
    public UnityEvent onJump;

    public Vector2 movementInput { get; private set; }
    public Vector3 movementVelocity { get; private set; }

    float lastTimeJumped;

    public float CurrentMoveSpeed
    {
        get
        {
            float speed = defaultSpeed;
            
            if (sprintController != null && sprintController.isSprinting)
            {
                speed *= sprintController.speedMultiplier;
            }
            else if (crouchController != null && crouchController.isCrouching)
            {
                speed *= crouchController.crouchSpeedMultiplier;
            }
            
            //Debug.Log(speed);
            return speed;
        }
    }


    //public void OnMove(InputValue input) => movementInput = canMove ? input.Get<Vector2>() : Vector2.zero;


    protected override void Awake()
    {
        base.Awake();

        movementBinding.onActionPerformed.AddListener(ProcessMovementInput);
        jumpBinding.onActionPerformed.AddListener(OnJump);
    }

    void ProcessMovementInput(InputAction.CallbackContext ctx)
    {
        movementInput = canMove ? ctx.ReadValue<Vector2>() : Vector2.zero;
    }
    void OnJump(InputAction.CallbackContext ctx)
    {
        if (groundingHandler.groundingData.collider == null && Time.time - lastTimeJumped >= jumpCooldown)
        {
            return;
        }

        if (crouchController != null)
        {
            crouchController.isCrouching = false;
        }
        rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        onJump.Invoke();
        lastTimeJumped = Time.time;
    }

    private void FixedUpdate()
    {
        Vector3 desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y) * CurrentMoveSpeed;
        movementVelocity = transform.TransformDirection(desiredVelocity);

        // Account for current velocity
        Vector3 currentVelocity = rigidbody.transform.InverseTransformDirection(rigidbody.velocity);
        desiredVelocity.y = currentVelocity.y;

        ShiftCharacterVelocityTowards(desiredVelocity, currentVelocity, acceleration, Space.Self);
    }
    private void OnDisable()
    {
        collider.material = standingMaterial;
    }
    
}