using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWalkCosmetics : MonoBehaviour
{
    public MovementController controller;
    public Transform upperBodyAnimationTransform;
    
    [Header("Walk Cycle")]
    public float strideLength = 1;
    public int stepsPerCycle = 2;
    public Vector2 bobExtents = new Vector2(0.2f, 0.1f);
    public AnimationCurve walkBobX;
    public AnimationCurve walkBobY;
    float walkCycleTimer;
    float stepTimer;

    [Header("Steps")]
    //public DiegeticAudioSource footsteps;
    //public float walkDecibels;
    //public float crouchDecibels;
    //public float sprintDecibels;
    public UnityEvent<RaycastHit> onStep;


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

    private void LateUpdate()
    {
        torsoPosition = Vector3.zero;
        torsoRotation = Quaternion.identity;

        #region Walk cycle
        // Implements bobbing animations for player walk cycle, and cosmetic effects whenever they take a step.

        // If player is on the ground and has EITHER started moving or stopped before the walk cycle finishes.
        if (controller.groundingData.collider != null && (controller.movementInput.magnitude > 0 || walkCycleTimer != 0))
        {
            float walkCycleLength = strideLength * stepsPerCycle / controller.CurrentMoveSpeed;
            float amountToIncrement = Time.deltaTime / walkCycleLength;
            walkCycleTimer += amountToIncrement;
            stepTimer += amountToIncrement;
            if (walkCycleTimer >= 1)
            {
                walkCycleTimer = 0;
            }
            if (stepTimer >= walkCycleLength / stepsPerCycle)
            {
                onStep.Invoke(controller.groundingData);
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

        #endregion

        #region Torso drag
        // Adds a cosmetic momentum drag to the player's hands when they are moving.

        float dragIntensity = Mathf.Clamp01(controller.TotalVelocity.magnitude / speedForMaxDrag);
        Vector3 dragOffset = -upperBodyDragDistance * dragIntensity * controller.TotalVelocity.normalized;
        //Vector3 dragOffset = Vector3.Lerp(Vector3.zero, -upperBodyDragDistance * TotalVelocity.normalized, dragIntensity);
        torsoPosition += controller.lookControls.aimAxis.InverseTransformDirection(dragOffset);

        #endregion

        #region Torso tilt
        // Adds cosmetic tilt to the player's hands when they move around.

        Vector3 localDirection = controller.transform.InverseTransformDirection(controller.TotalVelocity).normalized;
        Quaternion tilt = Quaternion.Euler(localDirection.z * upperBodyTiltAngle, 0, -localDirection.x * upperBodyTiltAngle);
        float tiltIntensity = Mathf.Clamp01(controller.TotalVelocity.magnitude / speedForMaxTilt);
        torsoRotation = Quaternion.Lerp(torsoRotation, torsoRotation * tilt, tiltIntensity);

        #endregion

        #region Torso sway
        // Adds cosmetic sway to the player's hands and held items when they turn and shift their aim.

        Quaternion localRotationVelocity = MiscFunctions.WorldToLocalRotation(controller.lookControls.rotationVelocity, controller.transform);
        float intensity = Mathf.Clamp01(localRotationVelocity.eulerAngles.magnitude / speedForMaxSway);
        Vector3 swayAxes = new Vector3(localRotationVelocity.x, localRotationVelocity.y, 0);
        swayAxes = Vector3.Lerp(Vector3.zero, swayAxes.normalized * -lookSwayDegrees, intensity);
        torsoRotation *= Quaternion.Euler(swayAxes);

        #endregion

        upperBodyAnimationTransform.localPosition = Vector3.SmoothDamp(upperBodyAnimationTransform.localPosition, torsoPosition, ref torsoMovementVelocity, torsoPositionUpdateTime);
        float timer = Mathf.SmoothDamp(0f, 1f, ref torsoAngularVelocityTimer, torsoRotationUpdateTime);
        upperBodyAnimationTransform.localRotation = Quaternion.Slerp(upperBodyAnimationTransform.localRotation, torsoRotation, timer);
    }

}
