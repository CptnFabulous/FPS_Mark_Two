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
    public Transform aimAxis;
    public Transform upperBody;
    public bool invertX;
    public bool invertY;

    [Header("Aiming - Mouse")]
    public float mouseSensitivity = 75;
    public float mouseMultiplierWhileAiming = 0.5f;

    [Header("Aiming - Gamepad")]
    public Vector2 gamepadSensitivity = new Vector2(25, 25);
    public float gamepadMultiplierWhileAiming = 0.25f;
    public float aimAcceleration = 6;
    public float timeToMaxAimAcceleration = 1;
    public AnimationCurve aimAccelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Camera")]
    public Camera worldViewCamera;
    public Camera headsUpDisplayCamera;
    [Range(1, 179)] public float fieldOfView = 90;

    float minAngle = -90;
    float maxAngle = 90;
    float verticalAngle = 0;

    public Vector2 rawAimInput { get; private set; }
    public Vector2 processedAimInput { get; private set; }
    public float aimStartTime { get; private set; }

    public Quaternion rotationVelocity => Quaternion.Euler(-processedAimInput.y, processedAimInput.x, 0) * transform.rotation;

    private void Awake()
    {
        worldViewCamera.fieldOfView = fieldOfView;
    }
    void Update()
    {
        processedAimInput = ProcessAimInput();
        if (processedAimInput.magnitude > 0)
        {
            RotateAim(processedAimInput * Time.deltaTime);
        }
    }

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

        bool usingGamepad = mainInput.currentControlScheme.Contains("Gamepad");
        bool inADS = weaponHandler != null && weaponHandler.IsUsingADS;

        #region Aim acceleration
        // If player is using a gamepad and out of ADS, apply mouse acceleration
        if (usingGamepad && !inADS)
        {
            float t = Time.time - aimStartTime; // Get how long the aim has been moving in a particular direction for
            t = Mathf.Clamp01(t / timeToMaxAimAcceleration); // Get a 0-1 value proportional to the desired time to reach max aim acceleration
            t = aimAccelerationCurve.Evaluate(t); // Multiply by curve
            t = Mathf.Lerp(1, aimAcceleration, t); // Use t to lerp between 1 and the max multiplier over time
            value.x *= t; // Multiply only on the X axis
        }
        #endregion

        #region Sensitivity
        // Apply sensitivity
        if (usingGamepad)
        {
            value.x *= gamepadSensitivity.x;
            value.y *= gamepadSensitivity.y;
        }
        else
        {
            value *= mouseSensitivity;
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
}
