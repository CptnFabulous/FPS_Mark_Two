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

    Weapon w;
    bool currentlyAiming;
    Vector3 cosmeticSwayAxes;
    Vector3 cosmeticSwayAngularVelocity;
    public float timer { get; private set; }

    Weapon attachedWeapon => w ??= GetComponentInParent<Weapon>();
    public Character user => attachedWeapon.user;
    WeaponHandler userWeaponHandler => user.weaponHandler;
    Player player => userWeaponHandler.controller;
    /// <summary>
    /// Is the player currently using ADS? Change this value to trigger ADS changing code
    /// </summary>
    public bool IsAiming
    {
        get => currentlyAiming;
        set
        {
            if (currentlyAiming == value) return;
            currentlyAiming = value;
            // Invoke either onSwitchToADS or onSwitchToHipfire based on if value is true or false
            (value ? onSwitchToADS : onSwitchToHipfire).Invoke();
        }
    }
    /// <summary>
    /// Is the player in the process of switching between hip-firing or aiming down the sights?
    /// </summary>
    public bool IsTransitioning => timer != TargetValue;
    float TargetValue => IsAiming ? 1 : 0;
    bool IsScope => viewingCamera != null && sightPicture != null;
    public LookController lookControls => player.movement.lookControls;
    bool notSetupProperly => attachedWeapon == null || user == null || userWeaponHandler == null || player == null;

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
    
    private void OnEnable()
    {
        if (notSetupProperly)
        {
            enabled = false;
            return;
        }
        //Debug.Log($"{this}: running OnEnable()");

        if (IsScope)
        {
            viewingCamera.fieldOfView = lookControls.fieldOfView / magnification;
        }
    }
    private void Update()
    {
        if (notSetupProperly)
        {
            enabled = false;
            return;
        }
        //Debug.Log($"{this}: running Update()");

        // If timer is different from desired value, lerp and update it
        if (IsTransitioning)
        {
            timer = Mathf.MoveTowards(timer, TargetValue, Time.deltaTime / transitionTime);
        }

        // If a view camera and sight picture are not present, directly lerp the player's FOV instead
        if (!IsScope)
        {
            float regularFOV = lookControls.fieldOfView;
            float zoomedFOV = regularFOV / magnification;
            lookControls.worldViewCamera.fieldOfView = Mathf.Lerp(regularFOV, zoomedFOV, timer);
        }

        Transform aimAxis = userWeaponHandler.aimAxis;
        Transform upperBody = lookControls.upperBody;
        Vector3 cameraDirection = Vector3.Slerp(aimAxis.forward, userWeaponHandler.AimDirection, timer);
        upperBody.LookAt(upperBody.position + cameraDirection, aimAxis.up);
    }
    private void LateUpdate()
    {
        if (notSetupProperly)
        {
            enabled = false;
            return;
        }
        //Debug.Log($"{this}: running LateUpdate()");

        if (IsAiming || IsTransitioning)
        {
            // Rotate gun so the reticle axis transform is parallel with the player's aim direction
            Quaternion relativeRotation = MiscFunctions.FromToRotation(modelPivot.rotation, reticleAxis.rotation);
            Quaternion rotation = lookControls.upperBody.rotation * Quaternion.Inverse(relativeRotation);
            modelOrientationTransform.rotation = Quaternion.Lerp(hipFireOrientation.rotation, rotation, timer);

            // If look sway values are greater than zero, run sway cosmetics
            // This check exists to prevent unnecessary processing, and also to prevent division by zero causing weird errors
            if (lookSwayDegrees != 0 && speedForMaxSway > 0)
            {
                // Modify gun rotation by sway value
                // Vector3.SmoothDamp is used on the euler angles for clean transitions.
                // If the smoothdamp is applied directly to the gun rotation, it drags too far behind.
                // Applying the base gun rotation straight then using smoothdamp on just the sway value is better at keeping the rotation within specified boundaries.
                Quaternion localRotationVelocity = MiscFunctions.WorldToLocalRotation(lookControls.rotationVelocity, player.movement.transform);
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
            Vector3 reticleRelativeToHead = lookControls.upperBody.forward * distanceBetweenReticleAxisAndHead;
            Vector3 position = lookControls.upperBody.position - reticleRelativeToModelTransform + reticleRelativeToHead;
            modelOrientationTransform.position = Vector3.Lerp(hipFireOrientation.position, position, timer);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(reticleAxis.position, -reticleAxis.forward * distanceBetweenReticleAxisAndHead);
        if (modelOrientationTransform != null)
        {
            Debug.DrawRay(modelOrientationTransform.position, modelOrientationTransform.forward, new Color(1, 0.5f, 0));
            Debug.DrawRay(modelOrientationTransform.position, modelOrientationTransform.up, new Color(1, 0.5f, 0));
        }
    }

}
