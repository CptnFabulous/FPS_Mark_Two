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

    protected Coroutine currentAttack;

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
    public bool inAttack => currentAttack != null;
    public virtual bool inSecondaryAction => false;
    public abstract LayerMask attackMask { get; }

    public abstract void OnSwitchTo();
    public abstract void OnSwitchFrom();

    public abstract bool CanAttack();
    protected abstract IEnumerator AttackSequence();
    public abstract void OnAttack();

    protected virtual void OnPrimaryInputChanged(bool held)
    {
        if (enabled == false) return;
        if (held == false) return;

        if (inAttack) return;
        if (CanAttack() == false) return;

        currentAttack = StartCoroutine(AttackSequence());
    }
    protected abstract void OnSecondaryInputChanged(bool held);
    public abstract void OnTertiaryInput();
    protected virtual void OnInterrupt() { }


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

    private void OnDisable()
    {
        SetPrimaryInput(false);
        SetSecondaryInput(false);

        if (currentAttack != null)
        {
            StopCoroutine(currentAttack);
            currentAttack = null;
        }

        OnInterrupt();
    }

}
