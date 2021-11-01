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

    public ButtonState GetStateFromInput(string input)
    {
        if (Input.GetButtonDown(input))
        {
            return ButtonState.Pressed;
        }
        else if (Input.GetButtonUp(input))
        {
            return ButtonState.Released;
        }
        else if (Input.GetButton(input))
        {
            return ButtonState.Held;
        }
        return ButtonState.Inactive;
    }

    public override void UpdateLoop(WeaponHandler user)
    {

        Debug.DrawRay(stats.muzzle.position, user.AimDirection(stats.sway) * 5, Color.red);



        // If fire button is pressed - bool
        // If number of shots in burst has not exceeded max burst - int
        // If enough ammunition remains in the magazine to perform the shot - int
        // If player is not currently reloading - bool in Magazine
        if (FireHeld && controls.InBurst == false)
        {
            StartCoroutine(controls.Fire(this, user));
        }



        // If reload button is pressed - bool OR If magazine is empty - int
        // If firing sequence has finished
        // If there is ammunition available to reload with - int
        // If player is not already reloading - bool in Magazine
        if (Input.GetButtonDown("Reload"))
        {
            Debug.Log("Initiating reload on frame " + Time.frameCount);
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
            


            return false;
        }
    }

    public bool CanShoot(WeaponHandler user)
    {
        if (magazine != null)
        {
            if (magazine.ammo.current < stats.ammoPerShot)
            {
                return false;
            }
        }

        if (stats.ammoType != null && stats.ammoPerShot > 0)
        {
            if (user.ammo.CurrentStock(stats.ammoType) < stats.ammoPerShot)
            {
                return false;
            }
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
        stats.Shoot(user.controller, user.aimOrigin.position, user.AimDirection(stats.sway), user.aimOrigin.up);
    }

}
