using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayOptions : OptionsMenu
{
    [Header("Options")]
    public Slider aimSensitivityX;
    public Slider aimSensitivityY;
    public float maxAimSensitivity = 500;
    public Toggle invertX;
    public Toggle invertY;
    public Slider fieldOfView;
    public Toggle toggleCrouch;
    public Toggle toggleADS;

    Player playerUpdating;
    
    public override void ApplySettings()
    {
        MovementController movement = playerUpdating.movement;
        movement.aimSensitivity.x = SliderValueToSensitivity(aimSensitivityX, maxAimSensitivity);
        movement.aimSensitivity.y = SliderValueToSensitivity(aimSensitivityY, maxAimSensitivity);
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

        SensitivityToSliderValue(aimSensitivityX, movement.aimSensitivity.x, maxAimSensitivity);
        SensitivityToSliderValue(aimSensitivityY, movement.aimSensitivity.y, maxAimSensitivity);
        invertX.isOn = movement.invertX;
        invertY.isOn = movement.invertY;
        fieldOfView.value = movement.fieldOfView;
        playerUpdating.movement.worldViewCamera.fieldOfView = fieldOfView.value;
        toggleCrouch.isOn = movement.toggleCrouch;

        toggleADS.isOn = playerUpdating.weapons.toggleADS;
    }

    public override void SetupOptions()
    {
        AddValueChangedEvent(aimSensitivityX);
        AddValueChangedEvent(aimSensitivityY);
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
