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
    public bool CanShoot(WeaponHandler user)
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
        if (stats.ammoType != null && stats.ammoPerShot > 0 && user.ammo.GetStock(stats.ammoType) < stats.ammoPerShot)
        {
            return false;
        }

        return true;
    }


    public override void UpdateLoop(WeaponHandler user)
    {
        if (optics != null)
        {
            optics.InputLoop(user);
        }
        
        if (user.primary.Pressed && controls.InBurst == false)
        {
            StartCoroutine(controls.Fire(this, user));
        }

        if (magazine != null)
        {
            magazine.InputLoop(this, user);
        }
    }

    public void SingleShot(WeaponHandler user)
    {
        if (magazine != null)
        {
            magazine.ammo.current -= stats.ammoPerShot;
        }

        if (stats.ammoType != null && stats.ammoPerShot > 0)
        {
            user.ammo.Spend(stats.ammoType, stats.ammoPerShot);
        }

        //Debug.Log("Shooting on frame " + Time.frameCount);
        stats.Shoot(user.controller, user.aimAxis.position, user.AimDirection(), user.aimAxis.up);
        //user.onAttack.Invoke(this);
    }

}
