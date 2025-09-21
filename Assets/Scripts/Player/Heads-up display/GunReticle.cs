using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class GunReticle : MonoBehaviour
{
    [SerializeField] WeaponHandler handler;
    [SerializeField] float targetRadius = 0.5f;
    [SerializeField] RectTransform centreDot;
    [SerializeField] RectTransform[] reticleBlades;
    [SerializeField] RectTransform simpleReticle;
    [SerializeField] AnimationCurve animationCurveForADS = AnimationCurve.EaseInOut(0, 1, 1, 0);

    RectTransform rt;
    CanvasGroup cg;
    Canvas rootCanvas;
    Vector2[] originalDirections;

    float currentAngle = float.MinValue;

    Camera playerCamera => handler.controller.movement.lookControls.worldViewCamera;
    ADSHandler adsHandler => handler.adsHandler;
    RangedAttack mode
    {
        get
        {
            Weapon w = handler.CurrentWeapon;
            if (w == null) return null;
            if (w.CurrentMode == null) return null;
            return w.CurrentMode as RangedAttack;
        }
    }
    GunADS ads => mode != null ? mode.optics : null;
    float opacity // A bit redundant but not a huge deal
    {
        get => cg.alpha;
        set => cg.alpha = value;
    }

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        cg = GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        originalDirections = new Vector2[reticleBlades.Length];
        for (int i = 0; i < reticleBlades.Length; i++)
        {
            originalDirections[i] = reticleBlades[i].anchoredPosition.normalized;
        }
    }
    private void LateUpdate()
    {
        // Check that a reticle is present
        if (mode == null)
        {
            // If not, hide the reticle and end the function early since there's nothing to render
            opacity = 0;
            simpleReticle.gameObject.SetActive(true);
            return;
        }
        // Deactivate simple reticle, so it doesn't get in the way of gun reticle
        simpleReticle.gameObject.SetActive(false);

        // Set visibility based on various factors specified in reticle opacity
        opacity = ReticleOpacity();
        // If reticle is not visible, don't bother updating other elements
        if (opacity <= 0) return;

        float angle = ReticleAngle();
        //if (angle == currentAngle) return;
        currentAngle = angle;

        centreDot.gameObject.SetActive(angle > 0);

        Vector3 origin = handler.aimAxis.position;

        // Use the spread angle and ideal target width to calculate a triangle
        // This tells us how close the target needs to be to fill the cone of fire
        // I used this calculator: https://www.omnicalculator.com/math/hypotenuse
        float hypotenuse = targetRadius / Mathf.Sin(angle * Mathf.Deg2Rad);
        float adjacent = hypotenuse * Mathf.Sin((90 - angle) * Mathf.Deg2Rad);
        Vector3 forwardStraight = handler.aimAxis.forward;
        Vector3 forwardAngled = handler.aimAxis.rotation * Quaternion.Euler(0, angle, 0) * Vector3.forward;
        Vector3 centreAtDistance = (adjacent * forwardStraight) + origin;
        Vector3 offsetAtDistance = (hypotenuse * forwardAngled) + origin;
        
        // Gain the screen positions of the centre and offset point
        Vector2 screenPosCentre = playerCamera.WorldToScreenPoint(centreAtDistance);
        Vector2 screenPosOffset = playerCamera.WorldToScreenPoint(offsetAtDistance);

        /*
        // TO DO: figure out a way to do this that doesn't involve RectTransformUtility.ScreenPointToLocalPointInRectangle()
        // E.g. directly converting based on the canvas and screen sizes
        // Figure out size of canvas during runtime compared to screen size, so you can ensure the values aren't warped
        // Figure out a conversion algorithm and call it something like ScreenToCanvasPoint
        */
        // Convert screen positions to the local space of the RectTransform
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : playerCamera;
        bool centreSuccess = RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPosCentre, cam, out screenPosCentre);
        bool offsetSuccess = RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPosOffset, cam, out screenPosOffset);
        //Debug.Log($"Centre = {centreSuccess}, {screenPosCentre}, offset = {offsetSuccess}, {screenPosOffset}");
        
        // Then calculate the distance and position reticle blades accordingly
        float screenDistance = Vector2.Distance(screenPosCentre, screenPosOffset);
        for (int i = 0; i < reticleBlades.Length; i++)
        {
            reticleBlades[i].anchoredPosition = originalDirections[i] * screenDistance;
        }
    }


    float ReticleOpacity()
    {
        // Do not show reticle if currently in the weapon selector
        if (handler.attackSelectors.menuIsOpen) return 0;

        // if no ADS, just make the reticle fully visible.
        if (ads == null) return 1;

        // If the reticle is never meant to be visible, show nothing
        if (ads.hideMainReticle) return 0;
        // If ADS does show reticle normally, have it lerp in visibility based on the ADS value.
        return Mathf.Lerp(0, 1, animationCurveForADS.Evaluate(adsHandler.timer));
    }
    float ReticleAngle()
    {
        float angle = mode.stats.spread;

        if (ads == null) return angle;

        // If ADS is present, animate angle so it shrinks when player activates it
        return Mathf.Lerp(0, angle, animationCurveForADS.Evaluate(adsHandler.timer));
        /*
        if (ads != null)
        {
            //angle = Mathf.Lerp(angle, 0, ads.timer);
            angle = Mathf.Lerp(0, angle, animationCurveForADS.Evaluate(ads.timer));
        }

        return angle;
        */
    }
}
