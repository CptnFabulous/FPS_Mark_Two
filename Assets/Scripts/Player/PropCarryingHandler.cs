using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PropCarryingHandler : MonoBehaviour
{
    public float maxWeight = 5;
    public UnityEvent<Rigidbody> onPickup;
    public UnityEvent<Rigidbody> onPickupFailed;
    public UnityEvent<Rigidbody> onThrow;

    public InteractionHandler interactionHandler;
    public ThrowHandler throwHandler;
    public SingleInput throwInput;

    WeaponHandler weaponHandler => throwHandler.user.weaponHandler;

    private void Awake()
    {
        // Add listeners to throw objects (these won't run if the player isn't carrying anything, due to checks in throwHandler)
        interactionHandler.input.onActionPerformed.AddListener((_) => Drop());
        throwInput.onActionPerformed.AddListener((value) =>
        {
            if (value.ReadValueAsButton() == true)
            {
                Throw();
            }
        });

        // Add listener to drop the current item if the player deliberately switches weapons
        weaponHandler.onSwitchWeapon.AddListener((_) => Drop(false));
    }

    public bool CanPickUpObject(Rigidbody target)
    {
        // Check that:
        // The player isn't already holding something
        if (throwHandler.holding != null) return false; 
        // There's something to pick up
        if (target == null) return false; 
        // The object can be picked up in the first place
        if (target.isKinematic) return false;
        if (target.mass > maxWeight) return false;

        return true;
    }
    public void Pickup(Rigidbody target)
    {
        // Check the size of the object: if it's small enough, add to inventory of quick throwables
        
        // Holster current weapon
        // (for now, later on I might have code for one-handing a gun with smaller objects, simply reducing accuracy instead)
        weaponHandler.SetCurrentWeaponActive(false);
        // Trigger pickup
        throwHandler.Pickup(target);
    }
    public void Drop(bool autoDrawLastWeapon = true)
    {
        throwHandler.Drop(out _);
        if (autoDrawLastWeapon) weaponHandler.SetCurrentWeaponActive(true);
    }
    public void Throw()
    {
        Rigidbody thrown = throwHandler.holding;
        throwHandler.Throw();
        onThrow.Invoke(thrown);
        weaponHandler.SetCurrentWeaponActive(true);
    }
}