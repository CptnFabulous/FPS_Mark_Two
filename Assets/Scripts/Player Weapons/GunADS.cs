using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunADS : MonoBehaviour
{
    [Header("Stats")]
    public float magnification = 1;
    public float transitionTime = 0.25f;
    public bool hideMainReticle;

    [Header("Sight picture")]
    public Camera viewingCamera;
    public UnityEngine.UI.Image sightPicture;
    public Vector2Int sightTextureDimensions = new Vector2Int(512, 512);
    public int sightTextureDepthBuffer = 1;

    [Header("Animations")]
    public Transform modelTransform;
    public Transform relativeHeadOrientation;
    public Transform weaponHipFireOrientation;

    public UnityEvent onSwitchToADS;
    public UnityEvent onSwitchToHipfire;

    Player player;
    bool currentlyAiming;
    float timer;

    /// <summary>
    /// Is the player currently using ADS? Change this value to trigger ADS changing code
    /// </summary>
    public bool IsAiming
    {
        get
        {
            return currentlyAiming;
        }
        set
        {
            if (currentlyAiming == value)
            {
                return;
            }

            if (value == true)
            {
                onSwitchToADS.Invoke();
            }
            else
            {
                onSwitchToHipfire.Invoke();
            }
            currentlyAiming = value;
        }
    }
    public bool IsTransitioning
    {
        get
        {
            return timer != TargetValue;
        }
    }
    float TargetValue
    {
        get
        {
            if (IsAiming)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
    bool IsScope
    {
        get
        {
            return viewingCamera != null && sightPicture != null;
        }
    }

    private void Awake()
    {
        if (IsScope)
        {
            RenderTexture sight = new RenderTexture(sightTextureDimensions.x, sightTextureDimensions.y, sightTextureDepthBuffer);
            viewingCamera.targetTexture = sight;
            sightPicture.material.mainTexture = sight;
        }
    }

    public void InputLoop(WeaponHandler user)
    {
        player = user.controller;
        if (IsScope)
        {
            viewingCamera.fieldOfView = player.movement.fieldOfView / magnification;
        }

        #region Controls
        if (IsAiming == false && user.secondary.Pressed)
        {
            IsAiming = true;
        }
        else if ((user.secondary.Released && user.toggleADS == false) || (user.secondary.Pressed && user.toggleADS == true))
        {
            IsAiming = false;
        }
        #endregion

        // If timer is different from desired value, lerp and update it
        if (IsTransitioning)
        {
            #region Calculate timer value
            float amountToAdd = Time.deltaTime / transitionTime;
            if (TargetValue < timer)
            {
                amountToAdd = -amountToAdd;
            }
            timer += amountToAdd;
            timer = Mathf.Clamp01(timer);
            #endregion

            #region Lerp values
            // If a view camera and sight picture are not present, directly lerp the player's FOV instead
            if (!IsScope)
            {
                float regularFOV = player.movement.fieldOfView;
                float zoomedFOV = regularFOV / magnification;
                player.movement.worldViewCamera.fieldOfView = Mathf.Lerp(regularFOV, zoomedFOV, timer);
            }
            #endregion
        }
        
        Vector3 cameraDirection = Vector3.Slerp(player.movement.aimAxis.forward, user.AimDirection(), timer);
        player.movement.upperBody.LookAt(player.movement.upperBody.position + cameraDirection);
    }

    private void LateUpdate()
    {
        if (IsAiming || IsTransitioning)
        {
            Quaternion relativeRotation = MiscFunctions.FromToRotation(modelTransform.rotation, relativeHeadOrientation.rotation);
            Quaternion rotation = player.movement.upperBody.rotation * Quaternion.Inverse(relativeRotation);
            modelTransform.rotation = Quaternion.Lerp(weaponHipFireOrientation.rotation, rotation, timer);
            Vector3 relativePosition = relativeHeadOrientation.position - modelTransform.position;
            Vector3 position = player.movement.upperBody.position - relativePosition;
            modelTransform.position = Vector3.Lerp(weaponHipFireOrientation.position, position, timer);
        }
    }

    /*
    public static bool ChangeMechanicActiveState(bool current, out bool updated, bool inputPressed, bool inputReleased, bool toggle)
    {
        if (inputPressed && toggle)
        {
            updated = !current;
            return true;
        }
        else
        {
            if (inputPressed)
            {
                updated = true;
                return true;
            }
            else if (inputReleased)
            {
                updated = false;
                return true;
            }
        }

        updated = current;
        return false;
    }
    */
}
