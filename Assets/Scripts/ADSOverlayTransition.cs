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
        if (ads.user == null || ads.player == null) return;

        overlayCanvas.worldCamera = ads.lookControls.headsUpDisplayCamera;

        // Enable overlay and disable weapon visuals, if past the desired threshold
        bool showOverlay = t > switchThreshold;
        reticleGroup.gameObject.SetActive(showOverlay);
        foreach (GameObject r in weaponModelComponents) r?.SetActive(!showOverlay);

        overlaySwapMask.alpha = swapMaskCurve.Evaluate(t);
    }
}