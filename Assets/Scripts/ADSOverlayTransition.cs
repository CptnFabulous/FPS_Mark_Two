using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADSOverlayTransition : MonoBehaviour
{
    public GunADS ads;
    [Range(0, 1)] public float switchThreshold = 0.5f;
    public GameObject[] weaponModelComponents;

    [Header("Canvas stuff")]
    public Canvas overlayCanvas;
    public float planeDistance = 0.35f;
    public RectTransform reticleGroup;
    public CanvasGroup overlaySwapMask;
    public AnimationCurve swapMaskCurve;

    private void Awake()
    {
        ads.onADSLerp.AddListener(OnLerp);
        overlayCanvas.planeDistance = planeDistance;
        OnLerp(0);
    }

    public void OnLerp(float t)
    {
        // Enable overlay and disable weapon visuals, if past the desired threshold
        bool showOverlay = t > switchThreshold;

        // If the ADS function is attached to a gun that's currently being used by a player, assign the world camera to the canvas
        if (showOverlay && ads.enabled && !ads.notSetupProperly)
        {
            overlayCanvas.worldCamera = ads.lookControls.headsUpDisplayCamera;
        }

        // Set visibility of overlay and weapon model
        reticleGroup.gameObject.SetActive(showOverlay);
        foreach (GameObject r in weaponModelComponents) r?.SetActive(!showOverlay);

        // Calculate swap mask opacity
        overlaySwapMask.alpha = swapMaskCurve.Evaluate(t);
    }
}