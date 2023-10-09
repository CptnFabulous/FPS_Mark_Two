using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponModeInfo : MonoBehaviour
{
    //public ResourceMeter magazineMeter;
    //public ResourceMeter ammoReserveMeter;
    public Text ammoText;
    public Text weaponModeName;
    public Image weaponModeIcon;

    public Image reloadMeter;

    [HideInInspector] public WeaponMode mode;

    private void LateUpdate()
    {
        bool somethingToShow = mode != null;
        enabled = somethingToShow;
        gameObject.SetActive(somethingToShow);
        if (enabled == false) return;

        // Display name and icon
        if (weaponModeName != null) weaponModeName.text = mode.name;
        if (weaponModeIcon != null) weaponModeIcon.sprite = mode.icon;

        // Perform different actions based on the mode
        switch (mode)
        {
            case RangedAttack r:
                UpdateAmmoText(r);
                break;
            case ThrowObject t:
                UpdateThrowableInfo(t);
                break;
        }
    }

    void UpdateAmmoText(RangedAttack rangedAttack)
    {
        ammoText.gameObject.SetActive(true);
        //magazineMeter.gameObject.SetActive(isValid);
        //ammoReserveMeter.gameObject.SetActive(isValid);

        ammoText.text = WeaponUtility.AmmoCounterHUDDisplay(rangedAttack, "Infinite");

        /*

        bool consumesAmmo = rangedAttack.consumesAmmo;
        bool feedsFromMagazine = rangedAttack.magazine != null;


        // If weapon consumes ammo, show reserve

        ammoReserveMeter.gameObject.SetActive(consumesAmmo);
        if (consumesAmmo)
        {
            Resource remainingAmmo = weapons.ammo.GetValues(rangedAttack.stats.ammoType);

            if (feedsFromMagazine) // If magazine is present, change ammo bar to show reserve excluding magazine amount
            {
                remainingAmmo.current -= rangedAttack.magazine.ammo.current;
                remainingAmmo.max -= (int)rangedAttack.magazine.ammo.max;
            }

            ammoReserveMeter.Refresh(remainingAmmo);
        }

        // If weapon has a magazine, show values
        magazineMeter.gameObject.SetActive(feedsFromMagazine);
        if (feedsFromMagazine)
        {
            magazineMeter.Refresh(rangedAttack.magazine.ammo);
        }
        */
    }
    void UpdateThrowableInfo(ThrowObject throwData)
    {
        ammoText.gameObject.SetActive(true);

        //int totalAmmo = Mathf.RoundToInt(mode.User.weaponHandler.ammo[throwData.ammunitionType].current);
        int totalAmmo = Mathf.RoundToInt(mode.User.weaponHandler.ammo.GetValues(throwData.ammunitionType).current);

        ammoText.text = $"{totalAmmo}";
    }
}