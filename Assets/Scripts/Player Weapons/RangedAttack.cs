using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : WeaponMode
{
    public GunGeneralStats stats;
    public GunFireController controls;
    public GunMagazine magazine;
    public GunADS optics;
    public override bool InAction
    {
        get
        {
            if (controls.InBurst)
            {
                return true;
            }

            if (magazine != null && magazine.ReloadActive)
            {
                return true;
            }

            if (optics != null)
            {
                // If player is currently aiming down sights, or still transitioning back to hipfiring
                if (optics.IsAiming == true || optics.IsTransitioning == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
    public bool NotReloading
    {
        get
        {
            return magazine == null || magazine.ReloadActive == false;
        }
    }

    public bool CanShoot()
    {
        // If gun feeds from a magazine, and there isn't enough ammo in the magazine to fire
        if (magazine != null)
        {
            // If not enough ammunition is in magazine to shoot, or the player is currently reloading
            if (magazine.ammo.current < stats.ammoPerShot || magazine.ReloadActive)
            {
                return false;
            }
        }

        // If the weapon consumes ammunition, but there isn't enough to fire
        if (stats.ConsumesAmmo && User.ammo.GetStock(stats.ammoType) < stats.ammoPerShot)
        {
            return false;
        }

        return true;
    }
    public void SingleShot()
    {
        if (magazine != null)
        {
            magazine.ammo.current -= stats.ammoPerShot;
        }
        if (stats.ConsumesAmmo)
        {
            User.ammo.Spend(stats.ammoType, stats.ammoPerShot);
        }

        stats.Shoot(User.controller, User.aimAxis.position, User.AimDirection, User.aimAxis.up);
    }

    public override void OnSwitchTo()
    {
        optics.Initialise(this);
        magazine.Initialise(this);
    }
    public override void OnSwitchFrom()
    {
        optics.enabled = false;
        magazine.enabled = false;
    }
    public override void OnPrimaryInputChanged()
    {
        if (PrimaryHeld && controls.InBurst == false && NotReloading)
        {
            StartCoroutine(controls.Fire(this));
        }
    }
    public override void OnSecondaryInputChanged()
    {
        if (optics == null)
        {
            return;
        }

        optics.IsAiming = SecondaryActive;
    }
    public override void OnTertiaryInput()
    {
        if (magazine != null)
        {
            magazine.OnReloadPressed();
        }
    }
}
