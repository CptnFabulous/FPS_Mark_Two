using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class WeaponMode : MonoBehaviour
{
    public string description = "A distinct attack mode for a weapon.";
    public Sprite icon;
    public float switchSpeed;
    public UnityEvent onSwitch;

    public Weapon attachedTo { get; private set; }
    public Character User => attachedTo.user;


    private void Awake()
    {
        attachedTo = GetComponentInParent<Weapon>();
    }
    public abstract void OnSwitchTo();
    public abstract void OnSwitchFrom();
    public abstract void OnPrimaryInputChanged();
    public abstract void OnSecondaryInputChanged();
    public abstract void OnTertiaryInput();
    public bool PrimaryHeld => User.weaponHandler.PrimaryHeld;
    public bool SecondaryActive => User.weaponHandler.SecondaryActive;

    public abstract bool InAction { get; }
}
