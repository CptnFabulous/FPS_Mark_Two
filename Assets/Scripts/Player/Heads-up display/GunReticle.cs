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

    RectTransform rt;
    CanvasGroup cg;
    Vector2[] originalDirections;

    Camera playerCamera => handler.controller.movement.worldViewCamera;
    
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
            SetVisibility(false);
            return;
        }
        #endregion

        #region Visibility
        // Disable reticle if player is aiming down weapon sights
        // Show reticle If ADS is null, OR if ADS is present but player is not aiming/transitioning and hideDefaultReticle is false
        GunADS ads = r.optics;
        bool notHiddenDueToAds = ads == null || (!(ads.IsAiming || ads.IsTransitioning) && !ads.hideMainReticle);
        bool notInWeaponSelector = handler.weaponSelector.menuIsOpen == false;
        bool reticleIsActive = notHiddenDueToAds && notInWeaponSelector;
        SetVisibility(reticleIsActive);

        // If reticle is invisible, don't bother updating other elements
        if (reticleIsActive == false) return;
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
    void SetVisibility(bool active) => cg.alpha = active ? 1 : 0;
}
