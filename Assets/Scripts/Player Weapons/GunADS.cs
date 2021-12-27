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
    public Transform hipFireOrientation;
    public Transform modelOrientationTransform;
    public Transform modelPivot;
    public Transform reticleAxis;
    public float distanceBetweenReticleAxisAndHead = 1f;
    public float lookSwayDegrees = 2;
    public float speedForMaxSway = 120;
    public float swayUpdateTime = 0.1f;
    public UnityEvent onSwitchToADS;
    public UnityEvent onSwitchToHipfire;

    
    public RangedAttack currentMode;
    WeaponHandler user
    {
        get
        {
            return currentMode.User;
        }
    }
    Player player
    {
        get
        {
            return user.controller;
        }
    }
    bool currentlyAiming;
    float timer;
    Vector3 cosmeticSwayAxes;
    Vector3 cosmeticSwayAngularVelocity;

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
    /// <summary>
    /// Is the player in the process of switching between hip-firing or aiming down the sights?
    /// </summary>
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
            // Generates a new render texture, and sets the scope camera to output to it rather than the main screen
            RenderTexture sight = new RenderTexture(sightTextureDimensions.x, sightTextureDimensions.y, sightTextureDepthBuffer);
            viewingCamera.targetTexture = sight;
            // Generates a unique material for this scope and assigns it to display the render texture
            Material scopeMaterial = new Material(sightPicture.material);
            scopeMaterial.mainTexture = sight;
            sightPicture.material = scopeMaterial;
        }
    }
    public void Initialise(RangedAttack mode)
    {
        currentMode = mode;
        if (IsScope)
        {
            viewingCamera.fieldOfView = player.movement.fieldOfView / magnification;
        }
        enabled = true;
    }

    private void Update()
    {
        if (currentMode == null)
        {
            return;
        }
        
        // If timer is different from desired value, lerp and update it
        if (IsTransitioning)
        {
            float amountToAdd = Time.deltaTime / transitionTime;
            if (TargetValue < timer)
            {
                amountToAdd = -amountToAdd;
            }
            timer += amountToAdd;
            timer = Mathf.Clamp01(timer);
        }

        // If a view camera and sight picture are not present, directly lerp the player's FOV instead
        if (!IsScope)
        {
            float regularFOV = player.movement.fieldOfView;
            float zoomedFOV = regularFOV / magnification;
            player.movement.worldViewCamera.fieldOfView = Mathf.Lerp(regularFOV, zoomedFOV, timer);
        }

        Vector3 cameraDirection = Vector3.Slerp(player.movement.aimAxis.forward, user.AimDirection(), timer);
        player.movement.upperBody.LookAt(player.movement.upperBody.position + cameraDirection);
    }
    private void LateUpdate()
    {
        if (IsAiming || IsTransitioning)
        {
            // Rotate gun so the reticle axis transform is parallel with the player's aim direction
            Quaternion relativeRotation = MiscFunctions.FromToRotation(modelPivot.rotation, reticleAxis.rotation);
            Quaternion rotation = player.movement.upperBody.rotation * Quaternion.Inverse(relativeRotation);
            modelOrientationTransform.rotation = Quaternion.Lerp(hipFireOrientation.rotation, rotation, timer);

            // If look sway values are greater than zero, run sway cosmetics
            // This check exists to prevent unnecessary processing, and also to prevent division by zero causing weird errors
            if (lookSwayDegrees != 0 && speedForMaxSway > 0)
            {
                // Modify gun rotation by sway value
                // Vector3.SmoothDamp is used on the euler angles for clean transitions.
                // If the smoothdamp is applied directly to the gun rotation, it drags too far behind.
                // Applying the base gun rotation straight then using smoothdamp on just the sway value is better at keeping the rotation within specified boundaries.
                Quaternion localRotationVelocity = MiscFunctions.WorldToLocalRotation(player.movement.RotationVelocity, player.movement.transform);
                float intensity = Mathf.Clamp01(localRotationVelocity.eulerAngles.magnitude / speedForMaxSway);
                Vector3 swayAxes = new Vector3(localRotationVelocity.x, localRotationVelocity.y, 0); // Only record X and Y values to prevent awkward shifting
                swayAxes = Vector3.Lerp(Vector3.zero, swayAxes.normalized * -lookSwayDegrees, intensity);
                cosmeticSwayAxes = Vector3.SmoothDamp(cosmeticSwayAxes, swayAxes, ref cosmeticSwayAngularVelocity, swayUpdateTime);
                modelOrientationTransform.localRotation *= Quaternion.Euler(cosmeticSwayAxes); // Apply sway on top of the regular rotation
            }

            //viewingCamera.transform.rotation = Quaternion.LookRotation(player.movement.upperBody.transform.forward, player.movement.upperBody.transform.up);

            // Calculate position of weapon model so reticle lines up with aim direction, outwards by distanceBetweenReticleAxisAndHead
            // This must be done after rotation, because rotating will change the relative position of the reticle axis
            Vector3 reticleRelativeToModelTransform = reticleAxis.position - modelPivot.position;
            Vector3 reticleRelativeToHead = player.movement.upperBody.forward * distanceBetweenReticleAxisAndHead;
            Vector3 position = player.movement.upperBody.position - reticleRelativeToModelTransform + reticleRelativeToHead;
            modelOrientationTransform.position = Vector3.Lerp(hipFireOrientation.position, position, timer);
        }

        Debug.DrawRay(modelOrientationTransform.position, modelOrientationTransform.forward, new Color(1, 0.5f, 0));
        Debug.DrawRay(modelOrientationTransform.position, modelOrientationTransform.up, new Color(1, 0.5f, 0));
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(reticleAxis.position, -reticleAxis.forward * distanceBetweenReticleAxisAndHead);
    }

}
