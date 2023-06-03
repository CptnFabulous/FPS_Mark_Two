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
    public Toggle toggleCrouch;
    public Toggle toggleADS;

    Player playerUpdating;

    MovementController movement => playerUpdating.movement;
    LookController lookControls => movement.lookControls;
    WeaponHandler weaponHandler => playerUpdating.weapons;

    public override void ApplySettings()
    {
        lookControls.mouseSensitivity = SliderValueToSensitivity(mouseSensitivity, maxMouseSensitivity);
        lookControls.gamepadSensitivity.x = SliderValueToSensitivity(gamepadSensitivityX, maxGamepadSensitivityX);
        lookControls.gamepadSensitivity.y = SliderValueToSensitivity(gamepadSensitivityY, maxGamepadSensitivityY);
        lookControls.invertX = invertX.isOn;
        lookControls.invertY = invertY.isOn;
        lookControls.fieldOfView = fieldOfView.value;

        movement.toggleCrouch = toggleCrouch.isOn;

        weaponHandler.toggleADS = toggleADS.isOn;
    }

    public override void ObtainCurrentValues()
    {
        playerUpdating = GetComponentInParent<Player>();
        if (playerUpdating == null)
        {
            return;
        }
        MovementController movement = playerUpdating.movement;

        SensitivityToSliderValue(mouseSensitivity, lookControls.mouseSensitivity, maxMouseSensitivity);
        SensitivityToSliderValue(gamepadSensitivityX, lookControls.gamepadSensitivity.x, maxGamepadSensitivityX);
        SensitivityToSliderValue(gamepadSensitivityY, lookControls.gamepadSensitivity.y, maxGamepadSensitivityY);
        invertX.isOn = lookControls.invertX;
        invertY.isOn = lookControls.invertY;
        fieldOfView.value = lookControls.fieldOfView;
        lookControls.worldViewCamera.fieldOfView = fieldOfView.value;
        toggleCrouch.isOn = movement.toggleCrouch;

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
                lookControls.worldViewCamera.fieldOfView = fov;
            }
        });
        AddValueChangedEvent(toggleCrouch);
        AddValueChangedEvent(toggleADS);
    }
}
