using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public Player controlling;

    #region Movement
    [Header("Movement")]
    public bool canMove = true;
    public float defaultSpeed = 5;
    CapsuleCollider collider;
    Rigidbody rb;
    public Vector3 movementVelocity { get; private set; }

    public void OnMove(InputValue input)
    {
        if (!canMove)
        {
            movementInput = Vector2.zero;
            return;
        }
        movementInput = input.Get<Vector2>();
    }
    Vector2 movementInput;

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
    Vector3 TotalVelocity
    {
        get
        {
            return rb.velocity + movementVelocity;
        }
    }
    #endregion

    #region Aiming

    [Header("Camera")]
    public bool canLook = true;
    public Transform aimAxis;
    public Transform upperBody;
    public Camera worldViewCamera;
    public Camera headsUpDisplayCamera;
    [Range(1, 179)] public float fieldOfView = 90;
    float minAngle = -90;
    float maxAngle = 90;

    float verticalAngle = 0;

    [Header("Aiming")]
    public float mouseSensitivity = 75;
    public float mouseMultiplierWhileAiming = 0.5f;
    public Vector2 gamepadSensitivity = new Vector2(25, 25);
    public float gamepadMultiplierWhileAiming = 0.25f;
    public bool invertX;
    public bool invertY;
    public float aimAcceleration = 6;
    public float timeToMaxAimAcceleration = 1;
    public AnimationCurve aimAccelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    void OnLook(InputValue input)
    {
        // Register zero if player looking is currently disabled
        if (!canLook)
        {
            rawAimInput = Vector2.zero;
            return;
        }

        Vector2 newInput = input.Get<Vector2>();

        // If the player has just started moving their aim horizontally (or switched to aiming in the opposite direction)
        float prevX = rawAimInput.x;
        float newX = newInput.x;
        if ((newX > 0 && prevX <= 0) || (newX < 0 && prevX >= 0))
        {
            // Reset aim time for aim acceleration calculations
            aimStartTime = Time.time;
        }

        rawAimInput = newInput; // Update rawAimInput to new value
    }
    Vector2 rawAimInput;
    float aimStartTime;

    public Vector2 AimInput
    {
        get
        {
            Vector2 value = rawAimInput; // Get raw input value
            if (value.magnitude <= 0) // If value is zero, no need to perform additional processing
            {
                return value;
            }

            // Apply aim acceleration, if gamepad is enabled and player is not aiming down sights
            bool usingGamepad = controlling.controls.currentControlScheme.Contains("Gamepad");
            //bool usingKeyboardAndMouse = controlling.controls.currentControlScheme.Contains("Keyboard&Mouse");
            bool inADS = controlling.weapons != null && controlling.weapons.IsUsingADS;
            if (usingGamepad)
            {
                if (inADS) // Apply ADS multiplier for easier aiming
                {
                    value *= gamepadMultiplierWhileAiming;
                }
                else // If player is using a gamepad and out of ADS, apply mouse acceleration
                {
                    float aimTime = Time.time - aimStartTime; // Get time between aim start and current time
                    float timeMultiplier = Mathf.Clamp01(aimTime / timeToMaxAimAcceleration); // Divide by timeToMaxAimAcceleration then clamp to a 0-1 value
                    timeMultiplier = aimAccelerationCurve.Evaluate(timeMultiplier); 
                    float aimAccelerationMultiplier = Mathf.Lerp(1, aimAcceleration, timeMultiplier); // Multiply by aim acceleration value and add one to get the aim multiplier
                    value.x *= aimAccelerationMultiplier; // Multiply aim input only on the X axis
                }

                value.x *= gamepadSensitivity.x;
                value.y *= gamepadSensitivity.y;
            }
            else
            {
                if (inADS) // Apply ADS multiplier for easier aiming
                {
                    value *= mouseMultiplierWhileAiming;
                }
                // Multiply by mouse sensitivity values
                value *= mouseSensitivity;
            }
            
            // Invert axes if appropriate
            if (invertX)
            {
                value.x = -value.x;
            }
            if (invertY)
            {
                value.y = -value.y;
            }

            return value;
        }
    }

    public Quaternion RotationVelocity
    {
        get
        {
            Vector2 cameraInput = AimInput;
            return Quaternion.Euler(-cameraInput.y, cameraInput.x, 0) * transform.rotation;
        }
    }

    public void RotateAim(Vector2 degrees)
    {
        verticalAngle -= degrees.y;
        verticalAngle = Mathf.Clamp(verticalAngle, minAngle, maxAngle);
        transform.Rotate(0, degrees.x, 0);
        aimAxis.localRotation = Quaternion.Euler(verticalAngle, 0, 0);
    }
    public IEnumerator RotateAimOverTime(Vector2 degrees, float time)
    {
        float timer = 0;
        while (timer != 1)
        {
            timer += Time.deltaTime / time;
            timer = Mathf.Clamp01(timer);
            RotateAim(degrees * Time.deltaTime / time);

            yield return new WaitForEndOfFrame();
        }
    }
    public IEnumerator RotateAimOverTime(Vector2 degrees, float time, AnimationCurve curve)
    {
        float timer = 0;
        float curveLastFrame = 0;

        while (timer != 1)
        {
            timer += Time.deltaTime / time;
            timer = Mathf.Clamp01(timer);

            float curveThisFrame = curve.Evaluate(timer);
            float curveDeltaTime = curveThisFrame - curveLastFrame;

            RotateAim(degrees * curveDeltaTime);

            yield return new WaitForEndOfFrame();
            curveLastFrame = curveThisFrame;
        }
    }
    #endregion

    #region Jumping
    [Header("Jumping")]
    public float jumpForce = 5;
    public float jumpCooldown = 0.1f;
    public float groundingRayLength = 0.01f;
    public UnityEvent onJump;
    public UnityEvent<RaycastHit> onLand;

    public RaycastHit groundingData;
    float lastTimeJumped;

    void OnJump()
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
            //Debug.Log("Landing on ground on frame " + Time.frameCount);
            onLand.Invoke(newGroundingData);
        }
        groundingData = newGroundingData; // Update grounding data
    }
    #endregion

    #region Dodging
    [Header("Dodging")]
    public float dodgeSpeed = 10;
    public float dodgeDistance = 5;
    public float dodgeCooldown = 1;
    public UnityEvent onDodge;
    float lastTimeDodged;
    void OnDodge()
    {
        // If player is standing on the ground
        // If cooldown time has elapsed
        // If player is moving in a direction
        if (groundingData.collider == null && Time.time - lastTimeDodged >= dodgeCooldown && movementInput.magnitude > 0)
        {
            return;
        }

        Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y) * dodgeDistance;
        movement = transform.rotation * movement;
        rb.AddForce(movement, ForceMode.Impulse);
        onDodge.Invoke();
        lastTimeDodged = Time.time;
    }
    #endregion

    #region Crouching
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

    void OnCrouch(InputValue input)
    {
        if (toggleCrouch)
        {
            if (input.isPressed)
            {
                IsCrouching = !IsCrouching;
            }
        }
        else
        {
            IsCrouching = input.isPressed;
        }
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
    #endregion

    #region Cosmetics
    [Header("Cosmetics")]
    public Transform upperBodyAnimationTransform;
    [Header("Walk Cycle")]
    public float strideLength = 1;
    public int stepsPerCycle = 2;
    public UnityEvent<RaycastHit> onStep;
    public Vector2 bobExtents = new Vector2(0.2f, 0.1f);
    public AnimationCurve walkBobX;
    public AnimationCurve walkBobY;
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

    /// <summary>
    /// Implements bobbing animations for player walk cycle, and cosmetic effects whenever they take a step.
    /// </summary>
    void WalkCycle()
    {
        // If player is on the ground and has EITHER started moving or stopped before the walk cycle finishes.
        if (groundingData.collider != null && (movementInput.magnitude > 0 || walkCycleTimer != 0))
        {
            float walkCycleLength = strideLength * stepsPerCycle / CurrentMoveSpeed;
            float amountToIncrement = Time.deltaTime / walkCycleLength;
            walkCycleTimer += amountToIncrement;
            stepTimer += amountToIncrement;
            if (walkCycleTimer >= 1)
            {
                walkCycleTimer = 0;
            }
            if (stepTimer >= walkCycleLength / stepsPerCycle)
            {
                onStep.Invoke(groundingData);
                stepTimer = 0;
            }

            // Add bobbing animations
            float bobX = walkBobX.Evaluate(walkCycleTimer) * bobExtents.x;
            float bobY = walkBobY.Evaluate(walkCycleTimer) * bobExtents.y;
            torsoPosition += new Vector3(bobY, bobX, 0);
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
        float dragIntensity = Mathf.Clamp01(TotalVelocity.magnitude / speedForMaxDrag);
        Vector3 dragOffset = -upperBodyDragDistance * dragIntensity * TotalVelocity.normalized;
        //Vector3 dragOffset = Vector3.Lerp(Vector3.zero, -upperBodyDragDistance * TotalVelocity.normalized, dragIntensity);
        torsoPosition += aimAxis.InverseTransformDirection(dragOffset);
    }
    /// <summary>
    /// Adds cosmetic tilt to the player's hands when they move around.
    /// </summary>
    void TorsoTilt()
    {
        Vector3 localDirection = transform.InverseTransformDirection(TotalVelocity).normalized;
        Quaternion tilt = Quaternion.Euler(localDirection.z * upperBodyTiltAngle, 0, -localDirection.x * upperBodyTiltAngle);
        float tiltIntensity = Mathf.Clamp01(TotalVelocity.magnitude / speedForMaxTilt);
        torsoRotation = Quaternion.Lerp(torsoRotation, torsoRotation * tilt, tiltIntensity);
    }
    /// <summary>
    /// Adds cosmetic sway to the player's hands and held items when they turn and shift their aim.
    /// </summary>
    void TorsoSway()
    {
        Quaternion localRotationVelocity = MiscFunctions.WorldToLocalRotation(RotationVelocity, transform);
        float intensity = Mathf.Clamp01(localRotationVelocity.eulerAngles.magnitude / speedForMaxSway);
        Vector3 swayAxes = new Vector3(localRotationVelocity.x, localRotationVelocity.y, 0);
        swayAxes = Vector3.Lerp(Vector3.zero, swayAxes.normalized * -lookSwayDegrees, intensity);
        torsoRotation *= Quaternion.Euler(swayAxes);
    }
    #endregion

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
        RotateAim(AimInput * Time.deltaTime);
    }
    private void FixedUpdate()
    {
        SetGroundingData();
        Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y) * CurrentMoveSpeed;
        //movementVelocity = transform.rotation * movement;
        
        
        movement = transform.rotation * movement;
        if (groundingData.collider != null)
        {
            // If grounded, rotates movement vector based on angle of surface so the player doesn't start falling by moving too fast off a downward slope.
            movement = Vector3.ProjectOnPlane(movement, groundingData.normal);
            Debug.DrawRay(transform.position, groundingData.normal, Color.magenta);
        }
        Debug.DrawRay(transform.position, movement);
        movementVelocity = movement;
        
        rb.MovePosition(transform.position + (movementVelocity * Time.fixedDeltaTime));
    }
    private void LateUpdate()
    {
        torsoPosition = Vector3.zero;
        torsoRotation = Quaternion.identity;

        WalkCycle();
        TorsoDrag();
        TorsoTilt();
        TorsoSway();

        upperBodyAnimationTransform.localPosition = Vector3.SmoothDamp(upperBodyAnimationTransform.localPosition, torsoPosition, ref torsoMovementVelocity, torsoPositionUpdateTime);
        float timer = Mathf.SmoothDamp(0f, 1f, ref torsoAngularVelocityTimer, torsoRotationUpdateTime);
        upperBodyAnimationTransform.localRotation = Quaternion.Slerp(upperBodyAnimationTransform.localRotation, torsoRotation, timer);
    }
}
