using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectorHUD : MonoBehaviour
{
    //public Image weaponImage;
    public Text weaponName;
    public Text firingModeName;
    public Text ammoCapacity;
    public Image ammoIcon;

    public void Refresh(RangedAttack mode)
    {
        firingModeName.text = mode.name;
        weaponName.text = mode.attachedTo.name;
        if (mode.stats.ammoType != null)
        {
            ammoIcon.sprite = mode.stats.ammoType.icon;
        }
        else
        {
            ammoIcon.sprite = null;
        }

        // If weapon consumes ammo, show reserve
        bool consumesAmmo = mode.stats.ConsumesAmmo;
        if (consumesAmmo)
        {
            int ammoCurrent = (int)mode.attachedTo.user.ammo.GetValues(mode.stats.ammoType).current;

            if (mode.magazine != null) // If magazine is present, change ammo bar to show reserve excluding magazine amount
            {
                int magazineCurrent = (int)mode.magazine.ammo.current;
                ammoCapacity.text = magazineCurrent + "/" + (ammoCurrent - magazineCurrent);
            }
            else
            {
                ammoCapacity.text = ammoCurrent.ToString();
            }
        }
        else
        {
            ammoCapacity.text = "INFINITE";
        }
    }
}
