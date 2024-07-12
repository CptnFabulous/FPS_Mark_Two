using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayOptions : OptionsMenu
{
    [Header("Options")]
    public Slider mouseSensitivity;
    public float maxMouseSensitivity = 500;
    public Slider gamepadSensitivityX;
    public float maxGamepadSensitivityX = 300;
    public Slider gamepadSensitivityY;
    public float maxGamepadSensitivityY = 150;
    public Toggle invertX;
    public Toggle invertY;
    public Slider fieldOfView;
    public Toggle toggleSprint;
    public Toggle toggleCrouch;
    public Toggle toggleADS;

    Player playerUpdating;

    MovementController movement => playerUpdating.movement;
    LookController lookControls => movement.lookControls;
    WeaponHandler weaponHandler => playerUpdating.weapons;

    public override void ApplySettings()
    {
        // Assign new sensitivity values
        lookControls.mouseSensitivityRange = (int)mouseSensitivity.value;
        lookControls.gamepadSensitivityRangeX = (int)gamepadSensitivityX.value;
        lookControls.gamepadSensitivityRangeY = (int)gamepadSensitivityY.value;
        // Assign other camera values
        lookControls.invertX = invertX.isOn;
        lookControls.invertY = invertY.isOn;
        lookControls.fieldOfView = fieldOfView.value;
        // Assign other control values
        if (movement.crouchController != null)
        {
            movement.crouchController.toggleCrouch = toggleCrouch.isOn;
        }
        if (movement.sprintController != null)
        {
            movement.sprintController.toggleInput = toggleSprint.isOn;
        }
        weaponHandler.toggleADS = toggleADS.isOn;
    }

    public override void ObtainCurrentValues()
    {
        playerUpdating = GetComponentInParent<Player>();
        if (playerUpdating == null) return;

        // Obtain sensitivity values
        mouseSensitivity.value = lookControls.mouseSensitivityRange;
        gamepadSensitivityX.value = lookControls.gamepadSensitivityRangeX;
        gamepadSensitivityY.value = lookControls.gamepadSensitivityRangeY;
        // Obtain other camera values
        invertX.isOn = lookControls.invertX;
        invertY.isOn = lookControls.invertY;
        fieldOfView.value = lookControls.fieldOfView;
        lookControls.currentFieldOfView = fieldOfView.value;
        // Obtain other control values
        toggleCrouch.isOn = (movement.crouchController != null) ? movement.crouchController.toggleCrouch : false;
        toggleSprint.isOn = (movement.sprintController != null) ? movement.sprintController.toggleInput : false;
        toggleADS.isOn = weaponHandler.toggleADS;
    }

    public override void SetupOptions()
    {
        AddValueChangedEvent(mouseSensitivity);
        AddValueChangedEvent(gamepadSensitivityX);
        AddValueChangedEvent(gamepadSensitivityY);
        AddValueChangedEvent(invertX);
        AddValueChangedEvent(invertY);
        AddValueChangedEvent(fieldOfView);
        fieldOfView.onValueChanged.AddListener((fov) =>
        {
            if (playerUpdating != null)
            {
                lookControls.currentFieldOfView = fov;
            }
        });
        AddValueChangedEvent(toggleSprint);
        AddValueChangedEvent(toggleCrouch);
        AddValueChangedEvent(toggleADS);
    }
}
