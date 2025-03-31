using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapCameraController : MonoBehaviour
{
    public MeshRenderer target;
    //public MapScreen mapScreen;
    public MeshFilter targetFilter;

    [Header("Camera")]
    public PlayerInput playerInput;
    public Camera camera;
    public Transform cameraAxisTransform;
    public float cameraLerpSpeed = 5;

    [Header("Panning")]
    public float panSensitivityX = 5;
    public float panSensitivityY = 5;

    [Header("Zooming")]
    public float zoomSensitivity = 5;

    [Header("Rotation")]
    public Vector2 rotateSensitivity = new Vector2(90, 90);
    public float cameraVerticalAngleLimit = 75;
    //public float angleMargin = 15;

    Vector3 panInput;
    Vector2 rotateInput;
    Vector3 _desiredCameraPosition = Vector3.zero;
    Quaternion _desiredCameraRotation = Quaternion.identity;

    public Vector3 defaultPosition { get; set; } = Vector3.zero;
    public Quaternion defaultRotation { get; set; } = Quaternion.identity;

    public Transform targetTransform => target.transform;
    public Bounds targetBounds => targetFilter.mesh.bounds;
    public float boundsMargin => 0;//targetBounds.extents.magnitude;
    public Vector3 desiredCameraPosition
    {
        get => _desiredCameraPosition;
        set
        {
            _desiredCameraPosition = value;
            //ClampCameraOrientation();
        }
    }
    public Quaternion desiredCameraRotation
    {
        get => _desiredCameraRotation;
        set
        {
            _desiredCameraRotation = value;
            //ClampCameraOrientation();
        }
    }
    public float deltaTime => Time.unscaledDeltaTime;
    public bool usingGamepad => playerInput.currentControlScheme.Contains("Gamepad");

    void OnNavigate(InputValue input)
    {
        Vector2 xy = input.Get<Vector2>();
        panInput.x = xy.x;
        panInput.y = xy.y;
    }
    void OnScrollWheel(InputValue input)
    {
        float zoomInput = input.Get<Vector2>().y;
        zoomInput = Mathf.Clamp(zoomInput, -1, 1);

        // If zooming using scroll wheel, don't allow input to get to zero from here (input is gradually reduced to zero in LateUpdate())
        if (!usingGamepad && Mathf.Abs(zoomInput) <= 0) return;
        // Assign new value
        panInput.z = zoomInput;
    }
    void OnPointerDelta(InputValue input)
    {
        Vector2 camera = input.Get<Vector2>();
        if (usingGamepad)
        {
            rotateInput = camera;
            return;
        }

        rotateInput = Vector2.zero;
        Rotate(camera);
    }
    void OnTertiaryAction()
    {
        RecentreCamera();
    }

    private void LateUpdate()
    {
        PanAndZoom(panInput);
        Rotate(rotateInput);

        if (!usingGamepad)
        {
            float scrollZoomReductionTime = 0.5f;
            panInput.z = Mathf.MoveTowards(panInput.z, 0, deltaTime / scrollZoomReductionTime);
        }
        
        ClampCameraOrientation();

        // Smoothly transition towards desired position
        float t = cameraLerpSpeed * deltaTime;
        cameraAxisTransform.localPosition = Vector3.Lerp(cameraAxisTransform.localPosition, desiredCameraPosition, t);
        cameraAxisTransform.localRotation = Quaternion.Lerp(cameraAxisTransform.localRotation, desiredCameraRotation, t);
        // Clamp camera rotation so it's still upright
        //cameraAxisTransform.localRotation = Quaternion.LookRotation(cameraAxisTransform.localRotation * Vector3.forward, Vector3.up);
    }

    public void PanAndZoom(Vector3 pan)
    {
        if (pan.sqrMagnitude <= 0) return;

        pan.x *= panSensitivityX;
        pan.y *= panSensitivityY;
        pan.z *= zoomSensitivity;
        pan *= targetBounds.extents.magnitude + boundsMargin;

        Vector3 direction = cameraAxisTransform.TransformDirection(pan);
        direction = targetTransform.InverseTransformDirection(direction);
        desiredCameraPosition += direction * deltaTime;
    }
    public void Rotate(Vector2 input)
    {
        input.x *= rotateSensitivity.x;
        input.y *= rotateSensitivity.y;
        input.y = -input.y;
        input *= deltaTime;
        desiredCameraRotation *= Quaternion.Euler(input.y, input.x, 0);
    }
    public void RecentreCamera()
    {
        desiredCameraPosition = defaultPosition;
        desiredCameraRotation = defaultRotation;
    }

    void ClampCameraOrientation()
    {
        // Clamp values to within a certain range of the object's bounds
        Vector3 closestPoint = targetBounds.ClosestPoint(_desiredCameraPosition);
        if (closestPoint != _desiredCameraPosition)
        {
            // Ensure camera can't move too far away from the object bounds
            float distance = Vector3.Distance(_desiredCameraPosition, closestPoint);
            if (distance > boundsMargin)
            {
                _desiredCameraPosition = Vector3.MoveTowards(_desiredCameraPosition, closestPoint, distance - boundsMargin);
            }
        }

        // TO DO: check if rotation would make camera upside down. If so, clamp it.
        Vector3 currentForward = _desiredCameraRotation * Vector3.forward;
        currentForward = Vector3.ProjectOnPlane(currentForward, Vector3.up);
        Quaternion previousMovementForward = Quaternion.LookRotation(currentForward, Vector3.up);
        float angle = Quaternion.Angle(_desiredCameraRotation, previousMovementForward);
        if (angle > cameraVerticalAngleLimit)
        {
            _desiredCameraRotation = Quaternion.RotateTowards(_desiredCameraRotation, previousMovementForward, angle - cameraVerticalAngleLimit);
        }

        // Clamp camera rotation so it's still upright
        _desiredCameraRotation = Quaternion.LookRotation(_desiredCameraRotation * Vector3.forward, Vector3.up);
    }
}