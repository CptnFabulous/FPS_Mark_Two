using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerThermalVision : MonoBehaviour
{

    [SerializeField] Camera mainCamera;
    [SerializeField] Camera hudCamera;

    UniversalAdditionalCameraData additionalData_main;
    UniversalAdditionalCameraData additionalData_hud;

    bool _thermalsActive;

    public bool thermalsActive
    {
        get => _thermalsActive;
        set => SetThermalsActive(value);
    }

    private void Awake()
    {
        additionalData_main = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        additionalData_hud = hudCamera.GetComponent<UniversalAdditionalCameraData>();
    }

    // TO DO: replace this crappy input with something compatible with the actual input system
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            thermalsActive = !thermalsActive;
        }
    }





    public void SetThermalsActive(bool active)
    {
        _thermalsActive = active;

        int rendererIndex = active ? 1 : 0;
        additionalData_main.SetRenderer(rendererIndex);
        additionalData_hud.SetRenderer(rendererIndex);
    }
}
