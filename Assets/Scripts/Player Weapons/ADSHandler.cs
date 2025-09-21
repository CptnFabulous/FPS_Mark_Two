using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADSHandler : MonoBehaviour
{
    public Player player;

    RangedAttack _currentWeapon = null;
    bool _aiming = false;
    Vector3 cosmeticSwayAxes;
    Vector3 cosmeticSwayAngularVelocity;
    public float timer { get; private set; }
    public float timerLastFrame { get; private set; }

    public RangedAttack currentAttack
    {
        get => _currentWeapon;
        set
        {
            // Cancel current ADS to prepare for data being scrubbed
            CancelADSImmediately();
            _currentWeapon = value;
            enabled = _currentWeapon != null;
        }
    }
    public WeaponHandler weaponHandler => player.weaponHandler;
    public GunADS adsData => currentAttack != null ? currentAttack.optics : null;
    public LookController lookControls => player.movement.lookControls;
    AimSwayHandler swayHandler => weaponHandler.swayHandler;
    Transform playerMovementTransform => player.movement.transform;
    Transform upperBody => lookControls.upperBody;
    Transform aimAxis => swayHandler.aimAxis;

    /// <summary>
    /// Is the player currently using ADS? Change this value to trigger ADS changing code
    /// </summary>
    public bool currentlyAiming
    {
        get => _aiming;
        set
        {
            bool desiredValue = value;

            // Don't allow it if weapon handler is currently disabling ADS
            value = (!weaponHandler.disableADS) && value;
            // Don't allow value to be true if ADS isn't even active
            value = enabled && value;

            //Debug.Log($"Attempting to set ADS of {adsData} to {desiredValue}, final = {value}. Disabled = {weaponHandler.disableADS}, enabled = {enabled}");

            if (_aiming == value) return;
            _aiming = value;
            // Invoke either onSwitchToADS or onSwitchToHipfire based on if value is true or false
            (value ? adsData.onSwitchToADS : adsData.onSwitchToHipfire).Invoke();
        }
    }
    public bool betweenStates => timerLastFrame != targetValue;
    float targetValue => currentlyAiming ? 1 : 0;

    private void Awake() => currentAttack = currentAttack;
    private void OnEnable() => timerLastFrame = timer;
    void OnDisable() => CancelADSImmediately();
    private void Update()
    {
        if (adsData == null) return;

        // If timer is different from desired value, lerp and update it
        timerLastFrame = timer;
        timer = Mathf.MoveTowards(timer, targetValue, Time.deltaTime / adsData.transitionTime);
        LerpADS(timer);
    }
    private void LateUpdate()
    {
        if (adsData == null) return;

        if (currentlyAiming || betweenStates) LerpADSCosmetics(timer);
    }

    /// <summary>
    /// Lerps the FOV and camera direction between standard and ADS modes.
    /// </summary>
    /// <param name="timer"></param>
    void LerpADS(float timer)
    {
        // Lerp FOV to desired value
        float regularFOV = lookControls.fieldOfView;
        float zoomedFOV = regularFOV / adsData.magnification;
        lookControls.currentFieldOfView = Mathf.Lerp(regularFOV, zoomedFOV, timer);

        Vector3 cameraDirection = Vector3.Slerp(aimAxis.forward, swayHandler.aimDirection, timer);
        upperBody.LookAt(upperBody.position + cameraDirection, aimAxis.up);
        
        // Lerp sway to change weapon accuracy while aiming down sights
        float sway = Mathf.Lerp(adsData.hipfireSwayMultiplier, swayHandler.adsMultiplier, timer);
        swayHandler.swayMultipliers[swayHandler.adsMultiplierReference] = sway;
    }
    /// <summary>
    /// Functions similarly to LerpADS, but runs in LateUpdate() for animations and visuals.
    /// </summary>
    /// <param name="timer"></param>
    void LerpADSCosmetics(float timer)
    {
        // Rotate gun so the reticle axis transform is parallel with the player's aim direction
        Quaternion relativeRotation = MiscFunctions.FromToRotation(adsData.modelPivot.rotation, adsData.reticleAxis.rotation);
        Quaternion rotation = lookControls.upperBody.rotation * Quaternion.Inverse(relativeRotation);
        adsData.modelOrientationTransform.rotation = Quaternion.Lerp(adsData.hipFireOrientation.rotation, rotation, timer);

        // If look sway values are greater than zero, run sway cosmetics
        // This check exists to prevent unnecessary processing, and also to prevent division by zero causing weird errors
        if (adsData.lookSwayDegrees != 0 && adsData.speedForMaxSway > 0)
        {
            // Modify gun rotation by sway value
            // Vector3.SmoothDamp is used on the euler angles for clean transitions.
            // If the smoothdamp is applied directly to the gun rotation, it drags too far behind.
            // Applying the base gun rotation straight then using smoothdamp on just the sway value is better at keeping the rotation within specified boundaries.
            Quaternion localRotationVelocity = MiscFunctions.WorldToLocalRotation(lookControls.rotationVelocity, playerMovementTransform);
            float intensity = Mathf.Clamp01(localRotationVelocity.eulerAngles.magnitude / adsData.speedForMaxSway);
            Vector3 swayAxes = new Vector3(localRotationVelocity.x, localRotationVelocity.y, 0); // Only record X and Y values to prevent awkward shifting
            swayAxes = Vector3.Lerp(Vector3.zero, swayAxes.normalized * -adsData.lookSwayDegrees, intensity);
            cosmeticSwayAxes = Vector3.SmoothDamp(cosmeticSwayAxes, swayAxes, ref cosmeticSwayAngularVelocity, adsData.swayUpdateTime);
            adsData.modelOrientationTransform.localRotation *= Quaternion.Euler(cosmeticSwayAxes); // Apply sway on top of the regular rotation
        }

        //viewingCamera.transform.rotation = Quaternion.LookRotation(player.movement.upperBody.transform.forward, player.movement.upperBody.transform.up);

        // Calculate position of weapon model so reticle lines up with aim direction, outwards by distanceBetweenReticleAxisAndHead
        // This must be done after rotation, because rotating will change the relative position of the reticle axis
        Vector3 reticleRelativeToModelTransform = adsData.reticleAxis.position - adsData.modelPivot.position;
        Vector3 reticleRelativeToHead = lookControls.upperBody.forward * adsData.distanceBetweenReticleAxisAndHead;
        Vector3 position = lookControls.upperBody.position - reticleRelativeToModelTransform + reticleRelativeToHead;

        float movementCurveTimer = adsData.modelMovementCurve.Evaluate(timer);
        adsData.modelOrientationTransform.position = Vector3.Lerp(adsData.hipFireOrientation.position, position, movementCurveTimer);

        adsData.onADSLerp.Invoke(this, timer);
    }
    void CancelADSImmediately()
    {
        if (currentAttack == null || adsData == null) return;

        currentlyAiming = false;
        timer = 0;

        LerpADS(0);
        LerpADSCosmetics(0);
        swayHandler.swayMultipliers[swayHandler.adsMultiplierReference] = 1;
    }
    public IEnumerator ChangeADSAsync(bool activate)
    {
        currentlyAiming = activate;
        yield return new WaitUntil(() => currentlyAiming == activate && !betweenStates);
    }
}