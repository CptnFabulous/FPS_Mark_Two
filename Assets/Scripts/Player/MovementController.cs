using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public Player controlling;

    public Rigidbody rigidbody => rb;
    public LayerMask collisionMask => MiscFunctions.GetPhysicsLayerMask(collider.gameObject.layer);

    #region Movement
    [Header("Movement")]
    public bool canMove = true;
    public float defaultSpeed = 5;
    public float acceleration = 50;
    public PhysicMaterial standingMaterial;
    public PhysicMaterial movingMaterial;

    [Header("Additional movement classes")]
    public LookController lookControls;
    public CrouchController crouchController;
    public SprintController sprintController;

    new CapsuleCollider collider;
    Rigidbody rb;
    public Vector2 movementInput { get; private set; }
    public Vector3 movementVelocity { get; private set; }

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
    public bool isGrounded => groundingData.collider != null;
    #endregion

    #region Jumping
    [Header("Jumping")]
    public float jumpForce = 5;
    public float jumpCooldown = 0.1f;
    public float groundingRayLength = 0.01f;
    public UnityEvent onJump;
    public UnityEvent<RaycastHit> onLand;

    public RaycastHit groundingData { get; private set; }
    float lastTimeJumped;

    void OnJump()
    {
        if (groundingData.collider == null && Time.time - lastTimeJumped >= jumpCooldown)
        {
            return;
        }

        if (crouchController != null)
        {
            crouchController.isCrouching = false;
        }
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        onJump.Invoke();
        lastTimeJumped = Time.time;
    }
    public static void GetGroundingData(CapsuleCollider collider, float groundingRayLength, out RaycastHit newGroundingData)
    {
        Transform transform = collider.transform;
        LayerMask collisionMask = MiscFunctions.GetPhysicsLayerMask(collider.gameObject.layer);

        Vector3 rayOrigin = transform.position + transform.up * (collider.height / 2);
        float distance = groundingRayLength + Vector3.Distance(transform.position, rayOrigin);
        float radius = collider.radius * 0.9f;
        Physics.SphereCast(rayOrigin, radius, -transform.up, out newGroundingData, distance, collisionMask);
    }
    #endregion

    public void OnMove(InputValue input) => movementInput = canMove ? input.Get<Vector2>() : Vector2.zero;
    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    private void FixedUpdate()
    {
        GetGroundingData(collider, groundingRayLength, out RaycastHit newGroundingData);
        if (newGroundingData.collider != null && groundingData.collider == null)
        {
            onLand.Invoke(newGroundingData);
        }
        groundingData = newGroundingData; // Update grounding data

        /*
        if (movementInput.sqrMagnitude > 0)
        {
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);

            movement = transform.TransformDirection(movement);
            // If grounded, rotates movement vector based on angle of surface so the player doesn't start falling by moving too fast off a downward slope.
            if (groundingData.collider != null) movement = Vector3.ProjectOnPlane(movement, groundingData.normal);
            movementVelocity = movement * CurrentMoveSpeed;

            rb.MovePosition(transform.position + (movementVelocity * Time.fixedDeltaTime));
        }
        */

        Vector3 currentVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        Vector3 desiredVelocity = new Vector3(movementInput.x, 0, movementInput.y) * CurrentMoveSpeed;
        movementVelocity = transform.TransformDirection(desiredVelocity);
        desiredVelocity.y = currentVelocity.y;
        ShiftCharacterVelocityTowards(desiredVelocity, currentVelocity, acceleration);
    }
    private void OnDisable()
    {
        collider.material = standingMaterial;
    }
    void ShiftCharacterVelocityTowards(Vector3 desired, Vector3 current, float acceleration)
    {
        // Swap out the player collider's material for moving versus standing still.
        // And cancel if the player doesn't want to move in a specific direction.
        bool wantsToMove = desired.sqrMagnitude > 0;
        collider.material = wantsToMove ? movingMaterial : standingMaterial;
        if (!wantsToMove) return;

        // Calculate the direction the velocity needs to shift in in order to reach the desired value (accounting for delta time)
        Vector3 accelerationVector = Vector3.MoveTowards(current, desired, acceleration * Time.fixedDeltaTime) - current;
        // Adjust velocity in desired direction
        rb.AddRelativeForce(accelerationVector, ForceMode.VelocityChange);

        //Debug.Log($"{desired}, {current}, {accelerationVector}");
    }
}