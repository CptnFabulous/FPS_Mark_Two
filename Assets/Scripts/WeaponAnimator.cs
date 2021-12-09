using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    public Weapon weaponToAnimate;
    public Animator controller;

    [Header("General Animator variables")]
    public string active = "Weapon Active";
    public string mode = "Firing Mode Index";
    public string modeSwitchTrigger = "Mode Switch Triggered";

    [Header("RangedAttack variables")]
    public string attackTrigger = "Firing";
    public string reloadActiveString = "Reload Active";
    public string reloadIncrementTrigger = "Next reload stage triggered";

    private void Awake()
    {
        weaponToAnimate.onDraw.AddListener(()=> controller.SetBool(active, true));
        weaponToAnimate.onHolster.AddListener(()=> controller.SetBool(active, false));
        
        for (int i = 0; i < weaponToAnimate.modes.Length; i++)
        {
            // Assign mode switch trigger to each mode
            WeaponMode m = weaponToAnimate.modes[i];
            m.onSwitch.AddListener(() => controller.SetTrigger(modeSwitchTrigger));
            // Assign shoot trigger to each firing mode
            RangedAttack rm = m as RangedAttack;
            if (rm != null)
            {
                rm.stats.effectsOnFire.AddListener(() => controller.SetTrigger(attackTrigger));
            }
        }

        // Assign reload trigger to each magazine in the weapon
        GunMagazine[] gm = weaponToAnimate.GetComponentsInChildren<GunMagazine>(true);
        for (int i = 0; i < gm.Length; i++)
        {
            gm[i].onIncrementStart.AddListener(() => controller.SetTrigger(reloadIncrementTrigger));
        }

    }

    private void Update()
    {
        //controller.SetBool(active, weaponToAnimate.isActiveAndEnabled);
        controller.SetInteger(mode, weaponToAnimate.currentModeIndex);

        RangedAttack rm = weaponToAnimate.CurrentMode as RangedAttack;
        if (rm != null)
        {
            bool isReloading = (rm.magazine != null && rm.magazine.ReloadActive == true);
            controller.SetBool(reloadActiveString, isReloading);
        }
        
        
    }
}
