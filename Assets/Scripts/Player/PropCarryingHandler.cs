using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PropCarryingHandler : WeaponMode
{
    public float maxWeight = 5;
    public float cooldown = 0.5f;
    public InteractionHandler interactionHandler;
    public ThrowHandler throwHandler;

    public UnityEvent<Rigidbody> onPickup;
    public UnityEvent<Rigidbody> onDrop;
    public UnityEvent<Rigidbody> onThrow;

    Rigidbody heldItem => throwHandler.holding;

    Rigidbody toPickUp;
    
    //bool pickupFinished => toPickUp == null;

    WeaponMode previousOffhandAbility;

    WeaponHandler weaponHandler => throwHandler.user.weaponHandler;
    OffhandAttackHandler offhandAttackHandler => weaponHandler.offhandAttacks;
    public override LayerMask attackMask => PhysicsUtility.GetPhysicsLayerMask(heldItem.gameObject.layer);
    public override string hudInfo => null;

    private void Awake()
    {
        // Ensure that correct code runs after dropping an item. The item drop code can be triggered from elsewhere
        throwHandler.onDrop.AddListener(OnDrop);
        enabled = false;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        // Drop currently held item (if there is one)
        throwHandler.CancelThrow();

        // If a previous offhand ability was stored, switch back to that
        if (previousOffhandAbility != null) offhandAttackHandler.currentMode = previousOffhandAbility;
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
        // Disallow if player is already holding an object
        if (heldItem != null)
        {
            //Debug.Log("Cannot pick up, player is already holding an object");
            return;
        }

        // TO DO: Check the size of the object: if it's small enough, add to inventory of quick throwables instead

        //Debug.Log($"Picking up {target}");
        toPickUp = target;

        // Reference offhand attack list, set active one to this
        WeaponMode currentlySelectedAbility = offhandAttackHandler.currentMode;
        if (currentlySelectedAbility != null)
        {
            previousOffhandAbility = currentlySelectedAbility;
        }
        StartCoroutine(offhandAttackHandler.DeployOffhandAttack(this));
    }
    public override IEnumerator SwitchTo()
    {
        // Trigger pickup
        yield return throwHandler.PickupSequence(toPickUp, switchSpeed);
        toPickUp = null;
        onPickup.Invoke(heldItem);
        // Enable cancel input even before attack starts, so player can easily switch away by dropping their current item
        offhandAttackHandler.cancelInput.enabled = true;
    }
    
    // Throwing/dropping object
    public override bool CanAttack() => heldItem != null; // TO DO: add stamina check later
    protected override IEnumerator AttackSequence()
    {
        Rigidbody thrown = throwHandler.holding;
        throwHandler.arcRenderer.accountForBounce = false;
        yield return throwHandler.Throw(() => PrimaryHeld);
        onThrow.Invoke(thrown);

        OnAttack();
    }
    public override void OnAttack()
    {
        // TO DO: potentially consume stamina
    }

    void OnDrop(Rigidbody dropped)
    {
        if (enabled == false) return;
        
        // Clear values
        toPickUp = null;

        onDrop.Invoke(dropped);

        enabled = false;
    }

    protected override void OnSecondaryInputChanged(bool held) { }
    public override void OnTertiaryInput() { }
}