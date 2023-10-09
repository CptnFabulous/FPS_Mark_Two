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

    Weapon _attached;
    Character _user;

    public Weapon attachedTo => _attached ??= GetComponentInParent<Weapon>();
    public Character User
    {
        get
        {
            if (attachedTo != null) return attachedTo.user;

            // Check for a new user if it has changed
            if (_user == null || transform.IsChildOf(_user.transform) == false)
            {
                _user = GetComponentInParent<Character>();
            }
            return _user;
        }
    }

    public abstract void OnSwitchTo();
    public abstract void OnSwitchFrom();
    protected abstract void OnPrimaryInputChanged(bool held);
    protected abstract void OnSecondaryInputChanged();
    public abstract void OnTertiaryInput();

    protected abstract void OnInterrupt();

    public bool PrimaryHeld { get; private set; }
    public bool SecondaryActive { get; private set; }

    public void SetPrimaryInput(bool held)
    {
        PrimaryHeld = held;
        OnPrimaryInputChanged(held);
    }
    public void SetSecondaryInput(bool active)
    {
        SecondaryActive = active;
        OnSecondaryInputChanged();
    }
    public void Interrupt()
    {
        SetPrimaryInput(false);
        SetSecondaryInput(false);
        OnInterrupt();
    }

    //public bool PrimaryHeld => User.weaponHandler.PrimaryHeld;
    //public bool SecondaryActive => User.weaponHandler.SecondaryActive;

    public abstract bool InAction { get; }
}
