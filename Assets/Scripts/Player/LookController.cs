using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookController : MonoBehaviour
{
    public PlayerInput mainInput;
    public WeaponHandler weaponHandler;

    [Header("Aiming")]
    public bool canLook = true;
    public Transform mainBodyTransform;
    public Transform aimAxis;
    public Transform upperBody;
    public bool invertX;
    public bool invertY;

    [Header("Mouse settings")]
    [SerializeField] float baseMouseSensitivity = 50;
    [Range(1, 10)] public int mouseSensitivityRange = 3;
    [Range(0.1f, 1)] public float mouseMultiplierWhileAiming = 0.5f;

    [Header("Gamepad settings")]
    [SerializeField] Vector2 baseGamepadSensitivity = new Vector2(50, 40);
    [Range(1, 10)] public int gamepadSensitivityRangeX = 3;
    [Range(1, 10)] public int gamepadSensitivityRangeY = 3;
    [SerializeField] AnimationCurve aimAccelerationCurve = AnimationCurve.Linear(0, 1, 0.5f, 1.75f);
    [SerializeField] float gamepadAccelerationStrengthThreshold = 0.75f;
    [Range(0.1f, 1)] public float gamepadMultiplierWhileAiming = 0.25f;

    [Header("Camera")]
    public Camera worldViewCamera;
    public Camera headsUpDisplayCamera;
    [Range(1, 179)] public float fieldOfView = 90;

    [Header("Recoil and offset")]
    public RecoilController recoilController;

    public float minAngle => -90;
    public float maxAngle => 90;

    Vector2 la;
    
    public Vector2 rawAimInput { get; private set; }
    public Vector2 processedAimInput { get; private set; }
    public float aimStartTime { get; private set; }
    public Vector2 lookAngles
    {
        get => la;
        set
        {
            la = value;
            la.y = Mathf.Clamp(la.y, minAngle, maxAngle);
        }
    }
    public Quaternion rotationVelocity => Quaternion.Euler(-processedAimInput.y, processedAimInput.x, 0) * transform.rotation;

    public bool usingGamepad => mainInput.currentControlScheme.Contains("Gamepad");

    private void Awake()
    {
        worldViewCamera.fieldOfView = fieldOfView;
    }
    private void OnEnable()
    {
        // Get local rotation of movement controller
        Vector2 newLookAngles = lookAngles;
        newLookAngles.x = mainBodyTransform.localEulerAngles.y;
        lookAngles = newLookAngles;
    }
    void Update()
    {
        #region Add player input
        processedAimInput = ProcessAimInput();
        float magnitude = processedAimInput.magnitude;
        if (magnitude > 0) lookAngles += processedAimInput * Time.deltaTime;
        #endregion

        #region Set actual rotation
        Vector2 totalAngles = lookAngles;
        if (recoilController != null)
        {
            totalAngles += recoilController.recoilValue;
        }

        Vector3 eulerAngles = mainBodyTransform.localEulerAngles;
        eulerAngles.y = totalAngles.x;
        mainBodyTransform.localEulerAngles = eulerAngles;
        aimAxis.localRotation = Quaternion.Euler(-totalAngles.y, 0, 0);
        #endregion
    }


    #region Private functions for processing direct input
    void OnLook(InputValue input)
    {
        // Register zero if player looking is currently disabled
        if (!canLook)
        {
            rawAimInput = Vector2.zero;
            return;
        }

        Vector2 newInput = input.Get<Vector2>();

        #region Reset aim time if direction changes
        // If the player has just started moving their aim horizontally (or switched to aiming in the opposite direction):
        // Reset aim time for aim acceleration calculations
        // I had to make a custom sign function because I want to know if the value is zero but Mathf.Sign will only return either 1 or -1.
        float SignWithZero(float input) => (input != 0) ? Mathf.Sign(input) : 0;
        if (SignWithZero(newInput.x) != SignWithZero(rawAimInput.x)) aimStartTime = Time.time;
        /*
        float prevX = rawAimInput.x;
        float newX = newInput.x;
        if ((newX > 0 && prevX <= 0) || (newX < 0 && prevX >= 0))
        */
        #endregion

        rawAimInput = newInput; // Update rawAimInput to new value
    }
    Vector2 ProcessAimInput()
    {
        // If value is zero, no need to perform additional processing
        if (rawAimInput.magnitude <= 0) return rawAimInput;

        Vector2 value = rawAimInput; // Get raw input value

        bool usingGamepad = this.usingGamepad;
        bool inADS = weaponHandler != null && weaponHandler.IsUsingADS;

        #region Aim acceleration
        // If player is using a gamepad and out of ADS, apply mouse acceleration
        if (usingGamepad && !inADS && rawAimInput.magnitude >= gamepadAccelerationStrengthThreshold)
        {
            value.x *= aimAccelerationCurve.Evaluate(Time.time - aimStartTime);
        }
        #endregion

        #region Sensitivity
        // Apply sensitivity
        if (usingGamepad)
        {
            value.x *= baseGamepadSensitivity.x * gamepadSensitivityRangeX;
            value.y *= baseGamepadSensitivity.y * gamepadSensitivityRangeY;
        }
        else
        {
            value *= baseMouseSensitivity * mouseSensitivityRange;
        }

        // Apply ADS multiplier for easier aiming
        if (inADS) value *= usingGamepad ? gamepadMultiplierWhileAiming : mouseMultiplierWhileAiming;
        #endregion

        #region Invert axes
        // Invert axes if appropriate
        if (invertX) value.x = -value.x;
        if (invertY) value.y = -value.y;
        #endregion

        return value;
    }
    #endregion
}