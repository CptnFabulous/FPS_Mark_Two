using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public Player controlling;
    public Transform aimAxis;
    public Transform upperBody;
    public Camera worldViewCamera;
    public Camera headsUpDisplayCamera;
    CapsuleCollider collider;
    Rigidbody rb;


    [Header("Movement")]
    public float defaultSpeed = 5;
    public UnityEvent<RaycastHit> onStep;
    float CurrentMoveSpeed
    {
        get
        {
            float speed = defaultSpeed;
            /*
            for (int i = 0; i < speedModifiers.Count; i++)
            {
                speed *= speedModifiers[i];
            }
            */
            speed *= Mathf.Lerp(1, crouchSpeedMultiplier, crouchTimer);
            //Debug.Log(speed);
            return speed;
        }
    }
    //List<float> speedModifiers = new List<float>();

    Vector3 distanceToMove;

    [Header("Aiming")]
    public float aimSensitivityX = 75;
    public float aimSensitivityY = 75;
    float minAngle = -90;
    float maxAngle = 90;
    float verticalAngle = 0;

    [Header("Jumping")]
    public float jumpForce = 5;
    public float jumpCooldown = 0.1f;
    public float groundingRayLength = 0.01f;
    public UnityEvent onJump;
    public UnityEvent<RaycastHit> onLand;

    public RaycastHit groundingData;
    float lastTimeJumped;

    [Header("Dodging")]
    public float dodgeSpeed = 10;
    public float dodgeDistance = 5;
    public float dodgeCooldown = 1;
    public UnityEvent onDodge;
    float lastTimeDodged;

    [Header("Crouching")]
    public bool toggleCrouch;
    public float standHeight = 2;
    public float crouchHeight = 1;
    public float headDistanceFromTop = 0.2f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchTransitionTime = 0.5f;
    public UnityEvent onCrouch;
    public UnityEvent onStand;
    [SerializeField] bool crouched;
    float crouchTimer;
    
    [Header("Cosmetics")]
    public float walkCycleLength = 0.5f;
    public int stepsPerCycle = 2;
    float walkCycleTimer;
    float stepTimer;



    Vector2 MovementInput
    {
        get
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            //Vector2 input = controlling.inputManager.actions.FindAction("Move").ReadValue<Vector2>();

            if (input.magnitude > 0) // Check before normalising, to allow proportional control while ensuring value does not exceed 1
            {
                input.Normalize();
            }
            return input;
        }
    }
    Vector2 CameraInput
    {
        get
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            //return controlling.inputManager.actions.FindAction("Look").ReadValue<Vector2>();
            //return controlling.inputManager.actions["Look"].ReadValue<Vector2>();
        }
    }


    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        IsCrouching = IsCrouching;
    }
    void Update()
    {
        SetGroundingData();
        
        if (Input.GetButtonDown("Crouch"))
        {
            IsCrouching = !IsCrouching;
        }
        
        Aim(CameraInput);
        Move(MovementInput);
        /*
        if (Input.GetButton("Dodge"))
        {
            TryDodge(movementInput);
        }
        */
        
        if (Input.GetButtonDown("Jump"))
        {
            TryJump();
        }
        
        
    }
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + (distanceToMove * Time.fixedDeltaTime));
        //distanceToMove = Vector3.zero; // Dispose of values once they have been applied
    }
    private void LateUpdate()
    {
        if (groundingData.collider != null && MovementInput.magnitude > 0)
        {
            walkCycleTimer += Time.deltaTime / walkCycleLength;
            stepTimer += Time.deltaTime / walkCycleLength * stepsPerCycle;
            //Debug.Log("Cycle: " + walkCycleTimer + ", step: " + stepTimer);
            if (walkCycleTimer > 1)
            {
                walkCycleTimer = 0;
            }
            if (stepTimer > 1)
            {
                onStep.Invoke(groundingData);
                stepTimer = 0;
            }
            //Debug.DrawRay(Vector3.zero, Vector3.up * walkCycleTimer, Color.blue);
            //Debug.DrawRay(Vector3.forward, Vector3.up * stepTimer, Color.red);

            // Add bobbing animations
        }
        else
        {
            walkCycleTimer = 0;
            stepTimer = 0;
        }
        
        


        
    }

    public void Move(Vector2 input)
    {
        Vector3 movement = new Vector3(input.x, 0, input.y) * CurrentMoveSpeed;
        movement = transform.rotation * movement;
        distanceToMove = movement;
    }
    public void Aim(Vector2 input)
    {
        float rotationH = input.x * aimSensitivityX * Time.deltaTime;
        float rotationV = input.y * aimSensitivityY * Time.deltaTime;

        verticalAngle -= rotationV;
        verticalAngle = Mathf.Clamp(verticalAngle, minAngle, maxAngle);
        transform.Rotate(0, rotationH, 0);
        aimAxis.localRotation = Quaternion.Euler(verticalAngle, 0, 0);
    }
    void SetGroundingData()
    {
        Vector3 rayOrigin = transform.position + transform.up * (collider.height / 2);
        float distance = groundingRayLength + Vector3.Distance(transform.position, rayOrigin);
        Physics.SphereCast(rayOrigin, collider.radius, -transform.up, out RaycastHit newGroundingData, distance, ~0);
        if (newGroundingData.collider != null && groundingData.collider == null)
        {
            //Debug.Log("Landing on ground on frame " + Time.frameCount);
            onLand.Invoke(newGroundingData);
        }
        groundingData = newGroundingData; // Update grounding data
    }
    public void TryJump()
    {
        if (groundingData.collider == null && Time.time - lastTimeJumped >= jumpCooldown)
        {
            return;
        }

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        onJump.Invoke();
        lastTimeJumped = Time.time;
    }
    /*
    public void TryJump(InputAction.CallbackContext context)
    {
        if (context.performed == false)
        {
            return;
        }
        
        if (groundingData.collider == null && Time.time - lastTimeJumped >= jumpCooldown)
        {
            return;
        }

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        onJump.Invoke();
        lastTimeJumped = Time.time;
    }
    */
    public void TryDodge(Vector2 input)
    {
        // If player is standing on the ground
        // If cooldown time has elapsed
        // If player is moving in a direction
        if (groundingData.collider == null && Time.time - lastTimeDodged >= dodgeCooldown && input.magnitude > 0)
        {
            return;
        }

        Vector3 movement = new Vector3(input.x, 0, input.y) * dodgeDistance;
        movement = transform.rotation * movement;
        rb.AddForce(movement, ForceMode.Impulse);
        onDodge.Invoke();
        lastTimeDodged = Time.time;
    }

    public bool IsCrouching
    {
        get
        {
            return crouched;
        }
        set
        {
            if (crouched == value) // Don't do anything if the player is already in the assigned state
            {
                return;
            }

            if (value == true)
            {
                crouched = true;
                StartCoroutine(Crouch());
            }
            else
            {
                if (true) // Check to ensure there is space above the player
                {
                    crouched = false;
                    StartCoroutine(Stand());
                }
            }
        }
    }
    IEnumerator Crouch()
    {
        onCrouch.Invoke();

        //speedModifiers.Add(crouchSpeedMultiplier);

        while (crouchTimer < 1)
        {
            LerpCrouch(crouchTimer);
            crouchTimer += Time.deltaTime / crouchTransitionTime;
            yield return null;
        }
    }
    IEnumerator Stand()
    {
        onStand.Invoke();

        while (crouchTimer > 0)
        {
            LerpCrouch(crouchTimer);
            crouchTimer -= Time.deltaTime / crouchTransitionTime;
            yield return null;
        }

        //speedModifiers.Remove(crouchSpeedMultiplier);
    }
    void LerpCrouch(float t)
    {
        collider.height = Mathf.Lerp(standHeight, crouchHeight, t);
        collider.center = Vector3.up * (collider.height / 2);
        aimAxis.transform.localPosition = new Vector3(0, collider.height - headDistanceFromTop, 0);
    }
}