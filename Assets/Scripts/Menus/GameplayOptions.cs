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
    
    public override void ApplySettings()
    {
        MovementController movement = playerUpdating.movement;
        movement.mouseSensitivity = SliderValueToSensitivity(mouseSensitivity, maxMouseSensitivity);
        movement.gamepadSensitivity.x = SliderValueToSensitivity(gamepadSensitivityX, maxGamepadSensitivityX);
        movement.gamepadSensitivity.y = SliderValueToSensitivity(gamepadSensitivityY, maxGamepadSensitivityY);
        movement.invertX = invertX.isOn;
        movement.invertY = invertY.isOn;
        movement.fieldOfView = fieldOfView.value;
        movement.toggleCrouch = toggleCrouch.isOn;

        playerUpdating.weapons.toggleADS = toggleADS.isOn;
    }

    public override void ObtainCurrentValues()
    {
        playerUpdating = GetComponentInParent<Player>();
        if (playerUpdating == null)
        {
            return;
        }
        MovementController movement = playerUpdating.movement;

        SensitivityToSliderValue(mouseSensitivity, movement.mouseSensitivity, maxMouseSensitivity);
        SensitivityToSliderValue(gamepadSensitivityX, movement.gamepadSensitivity.x, maxGamepadSensitivityX);
        SensitivityToSliderValue(gamepadSensitivityY, movement.gamepadSensitivity.y, maxGamepadSensitivityY);
        invertX.isOn = movement.invertX;
        invertY.isOn = movement.invertY;
        fieldOfView.value = movement.fieldOfView;
        playerUpdating.movement.worldViewCamera.fieldOfView = fieldOfView.value;
        toggleCrouch.isOn = movement.toggleCrouch;

        toggleADS.isOn = playerUpdating.weapons.toggleADS;
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
                playerUpdating.movement.worldViewCamera.fieldOfView = fov;
            }
        });
        AddValueChangedEvent(toggleCrouch);
        AddValueChangedEvent(toggleADS);
    }
}
