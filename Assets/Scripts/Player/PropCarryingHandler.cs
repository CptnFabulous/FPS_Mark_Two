using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PropCarryingHandler : MonoBehaviour
{
    public float maxWeight = 5;
    public float pickupTime = 0.15f;
    public float cooldown = 0.5f;
    public InteractionHandler interactionHandler;
    public ThrowHandler throwHandler;
    public SingleInput throwInput;

    public UnityEvent<Rigidbody> onPickup;
    public UnityEvent<Rigidbody> onDrop;
    public UnityEvent<Rigidbody> onThrow;

    int frameOfLastPickup = 0;

    Rigidbody heldItem;
    bool autoDrawLastWeaponOnDrop;

    WeaponHandler weaponHandler => throwHandler.user.weaponHandler;


    // TO DO:
    // Remove throw input, since that will be done by the offhand attack handler
    // Add an input to automatically drop the item and end the ability, if the player presses the fire button while their weapon is holstered.
    // When mode is enabled, store the previous offhand attack, and switch back to that once the object is thrown/dropped


    private void Awake()
    {
        // Add input to drop object
        interactionHandler.input.onActionPerformed.AddListener((_) => Drop(true, true));
        // Ensure that correct code runs after dropping an item. The item drop code can be triggered from elsewhere
        throwHandler.onDrop.AddListener(OnDrop);

        // Add input to throw object
        throwInput.onActionPerformed.AddListener((value) => Throw(value.ReadValueAsButton()));

        // Add listener to drop the current item if the player deliberately switches weapons
        weaponHandler.onSwitchWeapon.AddListener((_) => Drop(false, false));
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

        // If the target rigidbody belongs to an alive Character (whether player or AI controlled), the player shouldn't be able to pick it up
        Character c = EntityCache<Character>.GetEntity(target.gameObject);
        if (c != null && c.health.IsAlive) return false;

        return true;
    }
    
    public void Pickup(Rigidbody target)
    {
        // TO DO: Check the size of the object: if it's small enough, add to inventory of quick throwables instead

        // Reference offhand attack list, set active one to this

        StartCoroutine(PickupSequence(target));
    }
    

    public IEnumerator PickupSequence(Rigidbody target)
    {
        
        // Put away weapon (if it's two-handed)
        // This code is done in OffhandAttackHandler, but only when actually throwing. I added it here as well just in case
        if (weaponHandler.CurrentWeapon.oneHanded == false) yield return weaponHandler.SetCurrentWeaponDrawn(false);

        heldItem = target;
        frameOfLastPickup = Time.frameCount;

        // Trigger pickup
        yield return throwHandler.PickupSequence(target, pickupTime);


        onPickup.Invoke(target);
    }




    void Drop(bool frameCheck, bool autoDrawLastWeapon)
    {
        // A small hack fix to prevent the game from dropping an item the player just picked up due to the same input
        if (frameCheck && frameOfLastPickup == Time.frameCount) return;

        autoDrawLastWeaponOnDrop = autoDrawLastWeapon;
        throwHandler.Drop(out _);
    }
    void Throw(bool inputPressed = true)
    {
        if (!inputPressed) return;
        
        // TO DO: check for stamina, and consume stamina

        Rigidbody thrown = throwHandler.holding;
        throwHandler.Throw();
        weaponHandler.SetCurrentWeaponActive(true);
        onThrow.Invoke(thrown);
    }

    void OnDrop(Rigidbody dropped)
    {
        if (dropped != heldItem) return;


        // If specified prior, switch to the previous weapon
        // (can be disabled in case this code was run due to holstering current weapon)
        if (autoDrawLastWeaponOnDrop) weaponHandler.SetCurrentWeaponActive(true);

        // Clear values
        heldItem = null;
        autoDrawLastWeaponOnDrop = false;

        onDrop.Invoke(dropped);
    }

    private void OnDisable()
    {
        // Drop currently held item
        Drop(false, false);

        // TO DO: Switch back to last offhand attack
    }
}