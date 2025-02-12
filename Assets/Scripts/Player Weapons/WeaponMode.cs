using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class WeaponMode : MonoBehaviour
{
    [SerializeField] Weapon _attachedTo; // Don't reference directly, instead use 'attachedTo'
    public string description = "[PLACEHOLDER]";
    public Sprite icon;
    public float switchSpeed;
    public UnityEvent onSwitch;

    Character _user;
    public bool PrimaryHeld { get; protected set; }
    public bool SecondaryHeld { get; protected set; }

    public Weapon attachedTo => _attachedTo ??= GetComponentInParent<Weapon>();
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
    public abstract LayerMask attackMask { get; }

    public abstract void OnSwitchTo();
    public abstract void OnSwitchFrom();

    public abstract bool CanAttack();
    public abstract void OnAttack();

    protected abstract void OnPrimaryInputChanged(bool held);
    protected abstract void OnSecondaryInputChanged(bool held);
    public abstract void OnTertiaryInput();
    protected abstract void OnInterrupt();


    public void SetPrimaryInput(bool held)
    {
        PrimaryHeld = held;
        OnPrimaryInputChanged(held);
    }
    public void SetSecondaryInput(bool active)
    {
        SecondaryHeld = active;
        OnSecondaryInputChanged(active);
    }
    public void Interrupt()
    {
        SetPrimaryInput(false);
        SetSecondaryInput(false);
        OnInterrupt();
    }

    public abstract bool InAction { get; }
}
