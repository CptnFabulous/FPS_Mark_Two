using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PropCarryingHandler : WeaponMode
{
    public float maxWeight = 5;
    public float pickupTime = 0.15f;
    public float cooldown = 0.5f;
    public InteractionHandler interactionHandler;
    public ThrowHandler throwHandler;

    public UnityEvent<Rigidbody> onPickup;
    public UnityEvent<Rigidbody> onDrop;
    public UnityEvent<Rigidbody> onThrow;

    int frameOfLastPickup = 0;

    Rigidbody heldItem => throwHandler.holding;

    Rigidbody toPickUp;
    
    //bool pickupFinished => toPickUp == null;

    bool autoDrawLastWeaponOnDrop;

    WeaponMode previousOffhandAbility;

    WeaponHandler weaponHandler => throwHandler.user.weaponHandler;
    OffhandAttackHandler offhandAttackHandler => weaponHandler.offhandAttacks;
    public override LayerMask attackMask => MiscFunctions.GetPhysicsLayerMask(heldItem.gameObject.layer);

    // TO DO:
    // Add an input to automatically drop the item and end the ability, if the player presses the fire button while their weapon is holstered.

    private void Awake()
    {
        // Add input to drop object
        interactionHandler.input.onActionPerformed.AddListener((_) => Drop(true, true));
        // Ensure that correct code runs after dropping an item. The item drop code can be triggered from elsewhere
        throwHandler.onDrop.AddListener(OnDrop);

        //weaponHandler.primaryInput.onActionPerformed.AddListener(CheckToDropAndSwitchBackToWeapon);
        //weaponHandler.secondaryInput.onActionPerformed.AddListener(CheckToDropAndSwitchBackToWeapon);

        // Add listener to drop the current item if the player deliberately switches weapons
        weaponHandler.onSwitchWeapon.AddListener((_) => Drop(false, false));
    }
    private void OnDisable()
    {
        // Drop currently held item (if there is one)
        Drop(false, false);

        // If a previous offhand ability was stored, switch back to that
        if (previousOffhandAbility != null) offhandAttackHandler.currentAbility = previousOffhandAbility;
    }

    // Picking up object
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

        toPickUp = target;
        frameOfLastPickup = Time.frameCount;

        // Reference offhand attack list, set active one to this
        previousOffhandAbility = offhandAttackHandler.currentAbility;
        offhandAttackHandler.currentAbility = this;
        enabled = true;

        StartCoroutine(SwitchTo());
    }
    public override IEnumerator SwitchTo()
    {
        if (heldItem != null) yield break;

        // Put away weapon (if it's two-handed)
        // This code is done in OffhandAttackHandler, but only when actually throwing. I added it here as well just in case
        Weapon currentWeapon = weaponHandler.CurrentWeapon;
        if (currentWeapon != null && currentWeapon.oneHanded == false) yield return weaponHandler.SetCurrentWeaponDrawn(false);

        // Trigger pickup
        yield return throwHandler.PickupSequence(toPickUp, pickupTime);
        toPickUp = null;
        onPickup.Invoke(heldItem);
    }
    
    // Throwing/dropping object
    public override bool CanAttack() => heldItem != null; // TO DO: add stamina check later
    protected override IEnumerator AttackSequence()
    {
        Debug.Log("Throwing physics object");
        Rigidbody thrown = throwHandler.holding;
        throwHandler.Throw();
        weaponHandler.SetCurrentWeaponActive(true);
        onThrow.Invoke(thrown);

        OnAttack();
        yield break;
    }
    public override void OnAttack()
    {
        // TO DO: potentially consume stamina
    }
    /*
    void CheckToDropAndSwitchBackToWeapon(InputAction.CallbackContext context)
    {
        // This check only need apply to two-handed weapons, as they're the only ones being disabled
        if (weaponHandler.CurrentWeapon.oneHanded) return;

        if (context.ReadValueAsButton() == false) return;
        
        Drop(false, true);
    }
    */

    void Drop(bool frameCheck, bool autoDrawLastWeapon)
    {
        // A small hack fix to prevent the game from dropping an item the player just picked up due to the same input
        if (frameCheck && frameOfLastPickup == Time.frameCount) return;

        autoDrawLastWeaponOnDrop = autoDrawLastWeapon;
        throwHandler.Drop(out _);
    }
    void OnDrop(Rigidbody dropped)
    {
        if (enabled == false) return;
        
        // If specified prior, switch to the previous weapon
        // (can be disabled in case this code was run due to holstering current weapon)
        if (autoDrawLastWeaponOnDrop) weaponHandler.SetCurrentWeaponActive(true);

        // Clear values
        toPickUp = null;
        autoDrawLastWeaponOnDrop = false;

        onDrop.Invoke(dropped);

        enabled = false;
    }

    protected override void OnSecondaryInputChanged(bool held)
    {
        
    }

    public override void OnTertiaryInput()
    {
        
    }
}