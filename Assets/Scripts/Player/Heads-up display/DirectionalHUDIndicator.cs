using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DirectionalHUDIndicator : MonoBehaviour
{
    [Header("Position (uses topmost defined value)")]
    public Character targetCharacter;
    public Transform targetTransform;
    public Vector3 targetWorldSpace;

    [Header("Criteria to show")]
    [Range(10, 90)] public float minimumAngleToShow = 10;
    public bool checkIfOutsideViewport = true;

    [Header("Cosmetics")]
    public MaskableGraphic graphic;
    public bool rotate = true;
    public float distanceFromCentre = 100;
    public TimedCosmeticEffect animation;
    public bool destroyOnAnimationEnd = true;

    [Header("Setup")]
    public Player playerDirecting;
    public Transform centrePoint;
    public Camera ViewCamera
    {
        get
        {
            return playerDirecting.movement.worldViewCamera;
        }
    }
    public Vector3 TargetPosition
    {
        get
        {
            if (targetCharacter != null)
            {
                return targetCharacter.health.HitboxBounds.center;
            }
            else if (targetTransform != null)
            {
                return targetTransform.position;
            }
            else
            {
                return targetWorldSpace;
            }
        }
    }

    public void Setup(Player player, Transform centre)
    {
        playerDirecting = player;
        centrePoint = centre;
    }
    void LateUpdate()
    {
        if (destroyOnAnimationEnd && animation.completed)
        {
            Destroy(gameObject);
            return;
        }
        
        Vector3 position = TargetPosition;
        // Calculate world direction
        Vector3 direction = position - ViewCamera.transform.position;

        bool outsideThreshold = Vector3.Angle(direction, ViewCamera.transform.forward) > minimumAngleToShow;
        if (checkIfOutsideViewport)
        {
            // Calculate viewport position
            Vector3 p = ViewCamera.WorldToViewportPoint(position);
            // Checks if point is outside camera viewport
            bool pointIsOutsideViewport = p.x < 0 || p.x > 1 || p.y < 0 || p.y > 1;
            outsideThreshold = outsideThreshold || pointIsOutsideViewport;
        }
        graphic.enabled = outsideThreshold;
        if (!graphic.enabled)
        {
            return;
        }

        Debug.DrawRay(ViewCamera.transform.position, direction, Color.red);
        // Rotate world direction to account for a 2D HUD being used to represent 3D space

        //direction = Quaternion.Euler(-10, 0, 0) * direction;


        Debug.DrawRay(ViewCamera.transform.position, direction, Color.yellow);

        // Flatten to forward axis of camera
        direction = Vector3.ProjectOnPlane(direction, ViewCamera.transform.forward);
        // Convert to local space
        direction = ViewCamera.transform.InverseTransformDirection(direction);

        if (rotate)
        {
            transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);
        }
        transform.localPosition = centrePoint.localPosition + direction.normalized * distanceFromCentre;
    }
}
