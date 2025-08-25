using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WeaponAnimator : MonoBehaviour
{
    public Weapon weaponToAnimate;
    public Animator controller;
    //public RuntimeAnimatorController animationController;

    [Header("General Animator variables")]
    public string active = "Weapon Active";
    public string mode = "Firing Mode Index";
    public string modeSwitchTrigger = "Mode Switch Triggered";

    [Header("RangedAttack variables")]
    public string windupTrigger  = "Windup";
    public string attackTrigger = "Firing";
    public string isShooting = "Currently firing";
    public string reloadActiveString = "Reload Active";
    public string reloadIncrementTrigger = "Next reload stage triggered";

    [Header("Audio")]
    public AudioSource soundPlayer;
    public AudioClip[] weaponClips;

    [Header("Events")]
    public UnityEvent onEjection;

    //Transform weaponRoot => weaponToAnimate != null ? weaponToAnimate.transform : transform;
    Transform weaponRoot => weaponToAnimate.transform;

    private void Awake()
    {
        weaponToAnimate.onDraw.AddListener(() => controller.SetBool(active, true));
        weaponToAnimate.onHolster.AddListener(() => controller.SetBool(active, false));
        
        foreach (WeaponMode m in weaponToAnimate.modes)
        {
            // Assign mode switch trigger to each mode
            m.onSwitch.AddListener(() => controller.SetTrigger(modeSwitchTrigger));
            // Assign shoot trigger to each firing mode
            if (m is RangedAttack rm)
            {
                rm.onWindup.AddListener(() => controller.SetTrigger(windupTrigger));
                rm.onStartStopFiring.AddListener((b) => controller.SetBool(isShooting, b));
            }
        }

        // Assign attack trigger to each kind of attack
        foreach (RangedAttackFiringData firingData in weaponRoot.GetComponentsInChildren<GunGeneralStats>(true))
        {
            if ((firingData is GunGeneralStats stats) == false) continue;
            stats.effectsOnFire.AddListener(() => controller.SetTrigger(attackTrigger));
        }


        // Assign reload trigger to each magazine in the weapon
        foreach (GunMagazine gm in weaponRoot.GetComponentsInChildren<GunMagazine>(true))
        {
            gm.onIncrementStart.AddListener(() => controller.SetTrigger(reloadIncrementTrigger));
        }

    }
    private void Update()
    {
        //controller.SetBool(active, weaponToAnimate.isActiveAndEnabled);
        controller.SetInteger(mode, weaponToAnimate.currentModeIndex);

        RangedAttack rm = weaponToAnimate.CurrentMode as RangedAttack;
        if (rm != null)
        {
            bool isReloading = rm.currentlyReloading;
            controller.SetBool(reloadActiveString, isReloading);
        }


    }

    public void PlaySoundFromArray(int index)
    {
        if (weaponClips.Length <= 0)
        {
            return;
        }
        index = Mathf.Clamp(index, 0, weaponClips.Length - 1);
        soundPlayer.PlayOneShot(weaponClips[index]);
    }
    public void ShellEjection() => onEjection.Invoke();
}
