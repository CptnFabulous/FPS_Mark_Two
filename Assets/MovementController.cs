using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovementController : MonoBehaviour
{

    public Transform head;
    CapsuleCollider collider;
    Rigidbody rb;
    

    [Header("Movement")]
    public float defaultSpeed = 5;
    public UnityEvent onStep;
    Vector3 distanceToMove;
    float CurrentMoveSpeed
    {
        get
        {
            float speed = defaultSpeed;
            speed *= Mathf.Lerp(1, crouchSpeedMultiplier, crouchTimer);
            //Debug.Log(speed);
            return speed;
        }
    }



    [Header("Aiming")]
    public float aimSensitivityX = 75;
    public float aimSensitivityY = 75;
    float minAngle = -90;
    float maxAngle = 90;
    float verticalAngle;

    [Header("Jumping")]
    public float jumpForce = 5;
    public float jumpCooldown = 0.1f;
    public float groundingRayLength = 0.01f;
    public UnityEvent onJump;
    public UnityEvent onLand;

    public RaycastHit groundingData;
    float lastTimeJumped;

    [Header("Crouching")]
    public bool toggleCrouch;
    public float standHeight = 2;
    public float crouchHeight = 1;
    public float headDistanceFromTop = 0.2f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchTransitionTime = 0.5f;
    public UnityEvent onCrouch;
    public UnityEvent onStand;
    float crouchTimer;
    bool crouched;
    /*
    [Header("Cosmetics")]
    public float walkCycleLength = 0.5f;
    public int stepsPerCycle = 2;
    */




    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        IsCrouching = false;
    }
    void Update()
    {
        SetGroundingData();


        if (Input.GetButtonDown("Crouch"))
        {
            IsCrouching = !IsCrouching;
        }
        
        Vector2 cameraInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Aim(cameraInput);

        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Move(movementInput);
        
        if (Input.GetButtonDown("Jump"))
        {
            TryJump();
        }
    }
    private void FixedUpdate()
    {
        Vector3 amountToMove = (distanceToMove) * Time.fixedDeltaTime;
        rb.MovePosition(transform.position + amountToMove);
        //distanceToMove = Vector3.zero; // Dispose of values once they have been applied
    }
    /*
    private void LateUpdate()
    {
        float walkCycleTimer = Time.time % walkCycleLength;
        float stepTimer = walkCycleTimer % stepsPerCycle;
        Debug.Log("Cycle: " + walkCycleTimer + ", step: " + stepTimer);
    }
    */


    public void Move(Vector2 input)
    {
        if (input.magnitude > 0) // Check before normalising, to allow proportional control while ensuring value does not exceed 1
        {
            input.Normalize();
        }
        Vector3 movement = new Vector3(input.x, 0, input.y) * CurrentMoveSpeed;
        movement = transform.rotation * movement;
        distanceToMove = movement;
    }
    public void Aim(Vector2 input)
    {

        float rotationH = input.x * aimSensitivityX * Time.deltaTime;
        float rotationV = input.y * aimSensitivityY * Time.deltaTime;

        transform.Rotate(0, rotationH, 0);
        verticalAngle -= rotationV;
        verticalAngle = Mathf.Clamp(verticalAngle, minAngle, maxAngle);
        head.localRotation = Quaternion.Euler(verticalAngle, 0, 0);

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
    void SetGroundingData()
    {
        Vector3 rayOrigin = transform.position + transform.up * (collider.height / 2);
        float distance = groundingRayLength + Vector3.Distance(transform.position, rayOrigin);
        Physics.SphereCast(rayOrigin, collider.radius, -transform.up, out RaycastHit newGroundingData, distance, ~0);
        if (newGroundingData.collider != null && groundingData.collider == null)
        {
            Debug.Log("Landing on ground on frame " + Time.frameCount);
            onLand.Invoke();
        }
        groundingData = newGroundingData; // Update grounding data
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
    }
    void LerpCrouch(float t)
    {
        collider.height = Mathf.Lerp(standHeight, crouchHeight, t);
        collider.center = Vector3.up * (collider.height / 2);
        head.transform.localPosition = new Vector3(0, collider.height - headDistanceFromTop, 0);
    }

    
    /*
    bool SetFunctionState(string input, bool isToggled, bool originalState)
    {
        
    }
    */



    
}
