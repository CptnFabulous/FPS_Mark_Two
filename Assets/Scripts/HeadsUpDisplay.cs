using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeadsUpDisplay : MonoBehaviour
{
    public Player controller;

    [Header("Health")]
    public ResourceMeter healthMeter;

    public void UpdateHealthMeter(Health healthInfo)
    {
        healthMeter.Refresh(healthInfo.data);
    }


    [Header("Weapons")]
    public GameObject weaponInterface;
    public ResourceMeter magazineMeter;
    public ResourceMeter ammoReserve;
    public Text weaponModeName;
    public Image weaponModeIcon;

    public void ShowWeaponHUD(Weapon currentWeapon)
    {
        weaponInterface.gameObject.SetActive(true);
        SetWeaponModeFeatures(currentWeapon.CurrentMode);
    }
    public void HideWeaponHUD()
    {
        weaponInterface.gameObject.SetActive(false);
    }
    public void SetWeaponModeFeatures(WeaponMode currentMode)
    {
        weaponModeName.text = currentMode.name;
        weaponModeIcon.sprite = currentMode.icon;
        RefreshModeValues(currentMode);
    }
    public void RefreshModeValues(WeaponMode currentMode)
    {
        
        
        // Check if attack is a ranged mode
        RangedAttack rangedMode = currentMode as RangedAttack;
        magazineMeter.gameObject.SetActive(rangedMode != null);
        ammoReserve.gameObject.SetActive(rangedMode != null);
        if (rangedMode != null)
        {
            SetAmmunitionStatus(rangedMode);
        }
    }
    public void SetAmmunitionStatus(RangedAttack currentMode)
    {
        // If weapon consumes ammo, show reserve
        bool consumesAmmo = currentMode.stats.ammoType != null && currentMode.stats.ammoPerShot > 0;
        ammoReserve.gameObject.SetActive(consumesAmmo);
        if (consumesAmmo)
        {
            ammoReserve.gameObject.SetActive(true);
            Resource remainingAmmo = controller.weapons.ammo.GetValues(currentMode.stats.ammoType);

            if (currentMode.magazine != null)
            {
                remainingAmmo.current -= currentMode.magazine.ammo.current;
            }

            ammoReserve.Refresh(remainingAmmo);
        }

        // If weapon has a magazine, show values
        magazineMeter.gameObject.SetActive(currentMode.magazine != null);
        if (currentMode.magazine != null)
        {
            magazineMeter.Refresh(currentMode.magazine.ammo);
        }
    }
}