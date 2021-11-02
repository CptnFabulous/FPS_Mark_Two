using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : WeaponMode
{
    public GunGeneralStats stats;
    public GunFireController controls;
    public GunMagazine magazine;
    public GunOptics optics;



    
    public bool FirePressed
    {
        get
        {
            return Input.GetButtonDown("Fire");
        }
    }
    public bool FireHeld
    {
        get
        {
            return Input.GetButton("Fire");
        }
    }
    public bool FireReleased
    {
        get
        {
            return Input.GetButtonUp("Fire");
        }
    }

    

    public override void UpdateLoop(WeaponHandler user)
    {

        
        // If fire button is pressed - bool
        // If number of shots in burst has not exceeded max burst - int
        // If enough ammunition remains in the magazine to perform the shot - int
        // If player is not currently reloading - bool in Magazine
        if (FirePressed && controls.InBurst == false)
        {
            StartCoroutine(controls.Fire(this, user));
        }



        if (magazine != null)
        {
            if (magazine.WantsToReload(this) && magazine.CanReload(this, user))
            {
                Debug.Log("Initiating reload on frame " + Time.frameCount);
                magazine.StartReload();
            }
            else if (magazine.CurrentlyReloading && FirePressed)
            {
                Debug.Log("Cancelling reload");
                magazine.CancelReload();
            }
            
        }
        
    }

    

    




    public override bool InAction
    {
        get
        {
            if (controls.InBurst)
            {
                return true;
            }
            
            if (magazine != null && magazine.CurrentlyReloading)
            {
                return true;
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
            if (magazine.ammo.current < stats.ammoPerShot || magazine.CurrentlyReloading)
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
        user.onAttack.Invoke(this);
    }

}
