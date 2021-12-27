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
    
    [HideInInspector] public Weapon attachedTo;
    public WeaponHandler User
    {
        get
        {
            return attachedTo.user;
        }
    }
    
    private void Awake()
    {
        attachedTo = GetComponentInParent<Weapon>();
    }
    public abstract void OnSwitchTo();
    public abstract void OnSwitchFrom();
    public abstract void OnPrimaryInput();
    public abstract void OnSecondaryInput(bool held);
    public abstract void OnTertiaryInput();

    public abstract bool InAction { get; }
}
