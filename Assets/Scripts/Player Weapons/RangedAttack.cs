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

        Debug.DrawRay(stats.muzzle.position, user.AimDirection(stats.sway) * 5, Color.red);



        // If fire button is pressed - bool
        // If number of shots in burst has not exceeded max burst - int
        // If enough ammunition remains in the magazine to perform the shot - int
        // If player is not currently reloading - bool in Magazine
        if (FirePressed && controls.InBurst == false)
        {
            StartCoroutine(controls.Fire(this, user));
        }

        // If reload button is pressed - bool
        // If there is ammunition available to reload with - int
        // If player is not already reloading - bool in Magazine


        // If magazine is empty


        // If firing sequence has finished
        // If there is ammunition available to reload with

        
        if (Input.GetButtonDown("Reload"))
        {
            Debug.Log("Initiating reload on frame " + Time.frameCount);
        }
        
    }

    

    
    public bool AmmunitionAvailable
    {
        get
        {
            if (magazine != null)
            {
                return magazine.ammo.current < stats.ammoPerShot;
            }
            else
            {
                return true;
            }
        }
    }




    public override bool InAction
    {
        get
        {
            return false;
        }
    }



    public void SingleShot(WeaponHandler user)
    {
        if (magazine != null)
        {
            magazine.ammo.current -= stats.ammoPerShot;
        }

        //Debug.Log("Shooting on frame " + Time.frameCount);
        stats.Shoot(user.controller, user.aimOrigin.position, user.AimDirection(stats.sway), user.aimOrigin.up);
    }

}
