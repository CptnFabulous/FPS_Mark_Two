using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public Player controlling;

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
    CapsuleCollider collider;
    Rigidbody rb;

    [Header("Aiming")]
    public Transform aimAxis;
    public Transform upperBody;
    public Camera worldViewCamera;
    public Camera headsUpDisplayCamera;
    public float aimSensitivityX = 75;
    public float aimSensitivityY = 75;
    [Range(1, 179)] public float fieldOfView = 90;
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

    #region Cosmetics
    [Header("Cosmetics")]
    public Transform upperBodyAnimationTransform;
    public float walkCycleLength = 0.5f;
    public int stepsPerCycle = 2;
    float walkCycleTimer;
    float stepTimer;
    [Header("Drag")] // Torso lingering/dragging when moving
    public float upperBodyDragDistance = 0.2f;
    public float speedForMaxDrag = 20;
    [Header("Tilt")] // Torso leaning/tilting when moving
    public float upperBodyTiltAngle = 10;
    public float speedForMaxTilt = 20;
    [Header("Sway")] // Torso swaying/dragging when looking around
    public float lookSwayDegrees = 5;
    public float speedForMaxSway = 10;
    [Header("Return")] // Return to default position and rotation
    public float torsoPositionUpdateTime = 0.1f;
    public float torsoRotationUpdateTime = 0.1f;

    Vector3 torsoPosition;
    Quaternion torsoRotation;
    Vector3 torsoMovementVelocity;
    float torsoAngularVelocityTimer;
    #endregion

    Vector3 DeltaMovement
    {
        get
        {
            return transform.position - positionLastFrame;
        }
    }
    Vector3 positionLastFrame;
    Quaternion DeltaLookRotation
    {
        get
        {
            return lookRotationLastFrame * Quaternion.Inverse(upperBody.transform.rotation);
        }
    }
    Quaternion lookRotationLastFrame;


    Vector2 MovementInput
    {
        get
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        worldViewCamera.fieldOfView = fieldOfView;
    }
    void Start()
    {
        IsCrouching = IsCrouching;
    }
    void Update()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            IsCrouching = !IsCrouching;
        }

        InputAim(CameraInput);

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("Trying to jump");
            TryJump();
        }
    }
    private void FixedUpdate()
    {
        SetGroundingData();

        Vector2 input = MovementInput;
        Vector3 movement = new Vector3(input.x, 0, input.y) * CurrentMoveSpeed;
        movement = transform.rotation * movement;
        rb.MovePosition(transform.position + (movement * Time.fixedDeltaTime));
        /*
        if (groundingData.collider != null)
        {
            movement += Physics.gravity;
        }
        */

        /*
        if (movement.magnitude > 0)
        {
            float accelerationSpeed = 40f;
            Vector3 desiredHorizontalVelocity = 
            rb.velocity = Vector3.MoveTowards(rb.velocity, movement, accelerationSpeed * Time.fixedDeltaTime);
        }
        */

        

    }
    private void LateUpdate()
    {
        torsoPosition = Vector3.zero;
        torsoRotation = Quaternion.identity;

        WalkCycle();


        TorsoDrag();
        TorsoTilt();
        TorsoSway();

        upperBodyAnimationTransform.localPosition = Vector3.SmoothDamp(upperBodyAnimationTransform.localPosition, torsoPosition, ref torsoMovementVelocity, torsoPositionUpdateTime); ;
        float timer = Mathf.SmoothDamp(0f, 1f, ref torsoAngularVelocityTimer, torsoRotationUpdateTime);
        upperBodyAnimationTransform.localRotation = Quaternion.Slerp(upperBodyAnimationTransform.localRotation, torsoRotation, timer);


        positionLastFrame = transform.position;
        lookRotationLastFrame = upperBody.transform.rotation;
    }

    #region Aiming camera
    public void InputAim(Vector2 input)
    {
        float rotationH = input.x * aimSensitivityX * Time.deltaTime;
        float rotationV = input.y * aimSensitivityY * Time.deltaTime;
        Vector2 angles = new Vector2(rotationH, rotationV);
        if (angles.magnitude <= 0)
        {
            return;
        }
        RotateAim(angles);
    }
    public void RotateAim(Vector2 degrees)
    {
        verticalAngle -= degrees.y;
        verticalAngle = Mathf.Clamp(verticalAngle, minAngle, maxAngle);
        transform.Rotate(0, degrees.x, 0);
        aimAxis.localRotation = Quaternion.Euler(verticalAngle, 0, 0);
    }
    #endregion

    #region Jumping and dodging
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
        Debug.Log("Jumping");
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        onJump.Invoke();
        lastTimeJumped = Time.time;
    }
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
    #endregion

    #region Crouching
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
    #endregion

    void WalkCycle()
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
    /// <summary>
    /// Adds a cosmetic momentum drag to the player's hands when they are moving.
    /// </summary>
    void TorsoDrag()
    {
        if (Time.deltaTime <= 0)
        {
            return;
        }
        Vector3 totalVelocity = DeltaMovement / Time.deltaTime;
        float dragIntensity = Mathf.Clamp01(totalVelocity.magnitude / speedForMaxDrag);
        Vector3 direction = transform.InverseTransformDirection(totalVelocity);
        Vector3 dragMax = direction.normalized * -upperBodyDragDistance;
        Vector3 dragValue = Vector3.Lerp(Vector3.zero, dragMax, dragIntensity);
        torsoPosition += dragValue;
    }
    /// <summary>
    /// Adds cosmetic tilt to the player's hands when they move around.
    /// </summary>
    void TorsoTilt()
    {
        if (Time.deltaTime <= 0)
        {
            return;
        }
        Vector3 totalVelocity = DeltaMovement / Time.deltaTime;
        float tiltIntensity = Mathf.Clamp01(totalVelocity.magnitude / speedForMaxTilt);
        float tiltAngle = Mathf.Lerp(0, upperBodyTiltAngle, tiltIntensity);
        Vector3 newTiltDirection = Vector3.RotateTowards(transform.up, totalVelocity, tiltAngle * Mathf.Deg2Rad, 0);
        newTiltDirection = transform.InverseTransformDirection(newTiltDirection);
        torsoRotation *= Quaternion.FromToRotation(Vector3.up, newTiltDirection);
    }
    /// <summary>
    /// Adds cosmetic sway to the player's hands and held items when they turn and shift their aim.
    /// </summary>
    void TorsoSway()
    {
        float intensity = Mathf.Clamp01(DeltaLookRotation.eulerAngles.magnitude / speedForMaxSway);
        Vector3 swayAxes = new Vector3(DeltaLookRotation.x, DeltaLookRotation.y, DeltaLookRotation.z);
        swayAxes = Vector3.Lerp(Vector3.zero, swayAxes.normalized * lookSwayDegrees, intensity);
        torsoRotation *= Quaternion.Euler(swayAxes);
    }
    

}
