using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class GunReticle : MonoBehaviour
{
    [SerializeField] WeaponHandler handler;
    [SerializeField] float referenceDistance = 50;
    [SerializeField] RectTransform[] reticleBlades;
    [SerializeField] AnimationCurve opacityCurveForADS = AnimationCurve.EaseInOut(0, 1, 1, 0);

    RectTransform rt;
    CanvasGroup cg;
    Vector2[] originalDirections;

    Camera playerCamera => handler.controller.movement.worldViewCamera;
    float opacity // A bit redundant but not a huge deal
    {
        get => cg.alpha;
        set => cg.alpha = value;
    }

    private void Awake()
    {
        rt = GetComponent<RectTransform>();

        cg = GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        originalDirections = new Vector2[reticleBlades.Length];
        for (int i = 0; i < reticleBlades.Length; i++)
        {
            originalDirections[i] = reticleBlades[i].anchoredPosition.normalized;
        }
    }
    /*
    private void OnDisable()
    {
        SetVisibility(false);
    }
    */
    private void LateUpdate()
    {
        #region Check that a reticle is present
        RangedAttack r = handler.CurrentWeapon.CurrentMode as RangedAttack;
        if (r == null)
        {
            //enabled = false;
            opacity = 0;
            return;
        }
        #endregion

        #region Visibility
        // Set visibility based on various factors specified in reticle opacity
        opacity = ReticleOpacity(r);
        // If reticle is not visible, don't bother updating other elements
        if (opacity <= 0) return;
        #endregion

        #region Size
        float angle = handler.aimSwayAngle + r.stats.shotSpread;
        Quaternion offsetRotation = handler.aimAxis.rotation * Quaternion.Euler(angle, 0, 0);

        // Calculate direction and distance
        Vector3 centre = referenceDistance * handler.aimAxis.forward; // Straight forward
        Vector3 offset = referenceDistance * (offsetRotation * Vector3.forward); // Offset by the sway angle
        // Add onto the aim axis position to get the world space positions
        centre = handler.aimAxis.position + centre;
        offset = handler.aimAxis.position + offset;
        //Debug.DrawLine(handler.aimAxis.position, centre, Color.green);
        //Debug.DrawLine(handler.aimAxis.position, offset, Color.red);

        // Identify where they are on the screen and calculate the distance between them
        centre = playerCamera.WorldToScreenPoint(centre);
        offset = playerCamera.WorldToScreenPoint(offset);

        float screenDistance = Vector2.Distance(centre, offset);
        /*
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, centre, playerCamera, out Vector2 centrePosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, offset, playerCamera, out Vector2 offsetPosition);
        float canvasDistance = Vector2.Distance(centrePosition, offsetPosition);
        */
        // TO DO: Ensure the anchored positions scale with different screen sizes. I'm pretty sure the way they're set up the positions don't account for how the canvas size can be different from the screen size.
        for (int i = 0; i < reticleBlades.Length; i++)
        {
            //reticleBlades[i].anchoredPosition = originalDirections[i] * canvasDistance;
            reticleBlades[i].anchoredPosition = originalDirections[i] * screenDistance;
        }
        #endregion
    }

    float ReticleOpacity(RangedAttack r)
    {
        // Do not show reticle if currently in the weapon selector
        if (handler.weaponSelector.menuIsOpen) return 0;

        GunADS ads = r.optics;
        if (ads != null)
        {
            if (ads.hideMainReticle) // If the reticle is never meant to be visible, show nothing
            {
                return 0;
            }
            else // If ADS does show reticle normally, have it lerp in visibility based on the ADS value.
            {
                return opacityCurveForADS.Evaluate(ads.timer);
            }
        }

        return 1; // Make the reticle fully visible.
    }
}
