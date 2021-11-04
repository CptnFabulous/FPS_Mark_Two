using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunOptics : MonoBehaviour
{
    public float magnification;
    public float transitionTime;
    public Transform relativeHeadOrientation;
    public Transform weaponHipFireOrientation;

    public UnityEvent onSwitchToADS;
    public UnityEvent onSwitchToHipfire;


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

    public void InputLoop(RangedAttack mode, WeaponHandler user)
    {
        if (IsAiming == false && user.secondary.Pressed)
        {
            IsAiming = true;
        }
        else if ((user.secondary.Released && user.toggleADS == false) || (user.secondary.Pressed && user.toggleADS == true))
        {
            IsAiming = false;
        }

        MovementController movement = user.controller.movement;

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
            // Lerp FOV
            float regularFOV = movement.fieldOfView;
            float zoomedFOV = regularFOV / magnification;
            movement.worldViewCamera.fieldOfView = Mathf.Lerp(regularFOV, zoomedFOV, timer);

            // Lerp position of weapon model
            // Calculate position and rotation to lerp gun transform to so that relativeHeadOrientation is at the same orientation as the player's head
            //Vector3 adsPosition = MiscFunctions.MoveASoChildBMatchesC(gun.modelTransform.position, relativeHeadOrientation.position, user.aimAxis.position);
            //Quaternion adsRotation = MiscFunctions.RotateASoChildBMatchesC(gun.modelTransform.rotation, relativeHeadOrientation.rotation, user.aimAxis.rotation);


            // Hide weapon model when the ADS value reaches a certain threshold
            #endregion
        }
        
        Vector3 cameraDirection = Vector3.Slerp(movement.aimAxis.forward, user.AimDirection(), timer);
        movement.upperBody.LookAt(movement.upperBody.position + cameraDirection);
        
    }



}
