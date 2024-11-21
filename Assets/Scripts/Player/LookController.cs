using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookController : MonoBehaviour, ICharacterLookController
{
    public static float SignWithZero(float input) => (input != 0) ? Mathf.Sign(input) : 0;

    public PlayerInput mainInput;
    public WeaponHandler weaponHandler;

    [Header("Aiming")]
    public bool canLook = true;
    public Transform mainBodyTransform;
    public Transform aimAxis;
    public Transform upperBody;
    public bool invertX;
    public bool invertY;
    public PlayerAimAssist aimAssist;

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
    [SerializeField, Range(1, 179)] public float fieldOfView = 90;

    [Header("Recoil and offset")]
    public RecoilController recoilController;

    public float minAngle => -90;
    public float maxAngle => 90;

    public Vector2 rawAimInput { get; private set; }
    public float aimStartTime { get; private set; }

    public Quaternion rotationVelocity
    {
        get
        {
            Vector2 input = GetProcessedAimInput();
            return Quaternion.Euler(-input.y, input.x, 0) * transform.rotation;
        }
    }
    public bool usingGamepad => mainInput.currentControlScheme.Contains("Gamepad");

    public Vector2 lookAngles
    {
        get
        {
            Vector2 value = new Vector2(mainBodyTransform.eulerAngles.y, -aimAxis.localEulerAngles.x);
            while (value.y < -180) value.y += 360;
            while (value.y > 180) value.y -= 360;
            return value;
        }
        set
        {
            Vector3 eulerAngles = mainBodyTransform.localEulerAngles;
            eulerAngles.y = value.x;
            mainBodyTransform.localEulerAngles = eulerAngles;

            float clampedY = Mathf.Clamp(value.y, minAngle, maxAngle);
            aimAxis.localRotation = Quaternion.Euler(-clampedY, 0, 0);
        }
    }
    public bool active
    {
        get => enabled;
        set => enabled = value;
    }
    public Quaternion lookRotation
    {
        get => aimAxis.rotation;
        set
        {
            Vector3 lookDirection = value * Vector3.forward;
            // Obtains a flattened Vector3 value from lookRotation, to rotate the body on just one axis
            Vector3 transformDirection = Vector3.ProjectOnPlane(lookDirection, mainBodyTransform.up);
            mainBodyTransform.rotation = Quaternion.LookRotation(transformDirection, mainBodyTransform.up);
            // Rotates head to look in the appropriate direction
            aimAxis.rotation = value;
        }
    }

    public float currentFieldOfView
    {
        get => worldViewCamera.fieldOfView;
        set
        {
            worldViewCamera.fieldOfView = value;
            //headsUpDisplayCamera.fieldOfView = value;
        }
    }

    /// <summary>
    /// Returns the player's aim input, accounting for sensivity, inverted axes and aim acceleration.
    /// </summary>
    /// <returns></returns>
    public Vector2 GetProcessedAimInput()
    {
        // If value is zero, no need to perform additional processing
        if (rawAimInput.sqrMagnitude <= 0) return rawAimInput;

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
        if (inADS)
        {
            value *= usingGamepad ? gamepadMultiplierWhileAiming : mouseMultiplierWhileAiming;
            // Reduced further based on magnification
            value /= (weaponHandler.CurrentWeapon.CurrentMode as RangedAttack).optics.magnification;
        }
        #endregion

        #region Invert axes
        // Invert axes if appropriate
        if (invertX) value.x = -value.x;
        if (invertY) value.y = -value.y;
        #endregion

        return value;
    }

    /// <summary>
    /// Register raw input values and aim start time (for aim acceleration)
    /// </summary>
    /// <param name="input"></param>
    void OnLook(InputValue input)
    {
        if (!canLook)
        {
            rawAimInput = Vector2.zero;
            return;
        }

        Vector2 newInput = input.Get<Vector2>();

        // Reset player aim time on start or direction change, for aim acceleration calculations
        if (SignWithZero(newInput.x) != SignWithZero(rawAimInput.x)) aimStartTime = Time.time;

        rawAimInput = newInput; // Update rawAimInput to new value
    }

    void Start()
    {
        currentFieldOfView = fieldOfView;
    }
    void Update()
    {
        Vector2 delta = GetProcessedAimInput();
        if (delta.sqrMagnitude > 0)
        {
            delta *= Time.deltaTime;
            lookAngles += delta;
        }
    }
}