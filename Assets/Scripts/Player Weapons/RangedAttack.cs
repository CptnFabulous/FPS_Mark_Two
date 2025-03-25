using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : WeaponMode
{
    public GunGeneralStats stats;
    public GunFireController controls;
    public GunMagazine magazine;
    public GunADS optics;

    public override LayerMask attackMask => stats.projectilePrefab.detection;
    public int shotsInBurst { get; private set; }

    public override bool inSecondaryAction
    {
        get
        {
            if (currentlyReloading) return true;

            if (optics != null)
            {
                // If player is currently aiming down sights, or still transitioning back to hipfiring
                if (optics.IsAiming || optics.IsTransitioning) return true;
            }

            return false;
        }
    }
    public bool currentlyReloading => magazine != null && magazine.currentlyReloading;

    public AmmunitionInventory ammo => (User != null && User.weaponHandler != null) ? User.weaponHandler.ammo : null;
    public bool consumesAmmo => ammo != null && stats.ammoType != null && stats.ammoPerShot > 0;

    public override string hudInfo => WeaponUtility.AmmoCounterHUDDisplay(this);


    private void OnEnable()
    {
        if (optics != null) optics.enabled = true;

        magazine.modeServing = this;
        magazine.enabled = true;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        optics.enabled = false;
        magazine.enabled = false;
    }

    protected override void OnSecondaryInputChanged(bool held)
    {
        if (optics == null) return;
        WeaponHandler handler = User.weaponHandler;
        if (handler == null) return;

        bool desiredState = MiscFunctions.GetToggleableInput(optics.IsAiming, held, handler.toggleADS);
        Debug.Log($"Attempting to set ADS to {desiredState}, disabled = {handler.disableADS}");
        optics.IsAiming = (!handler.disableADS) && desiredState;
    }
    public override void OnTertiaryInput()
    {
        if (magazine == null) return;
        //magazine.OnReloadPressed();

        if (!magazine.currentlyReloading)
        {
            magazine.TryReload();
        }
        else
        {
            magazine.CancelReload();
        }
    }


    /// <summary>
    /// Continuously fires shots until the burst timer is reached, the gun runs out of ammo, or the player lets go of the trigger.
    /// </summary>
    protected override IEnumerator AttackSequence()
    {
        if (magazine != null && magazine.currentlyReloading)
        {
            magazine.CancelReload();
            yield break;
        }
        
        shotsInBurst = 0;
        float timeOfLastMessage = Mathf.NegativeInfinity; // Sets up the message timer to infinity, to ensure it always sends a message on the first shot.

        while (CanAttack()) // Check stuff like fire button held, ammo remaining
        {
            TrySendMessage(ref timeOfLastMessage);
            
            yield return SingleShot(); // Fire shot and increment burst timer
        }

        // Wait for the cooldown, if applicable
        float cooldown = controls.burstCooldown;
        if (cooldown > 0) yield return new WaitForSeconds(cooldown);

        // Wait until the fire button is released
        yield return new WaitUntil(() => PrimaryHeld == false);

        // Reset shot timer
        shotsInBurst = 0;
        currentAttack = null;
    }
    /// <summary>
    /// Fires a single shot and increments the burst counter.
    /// </summary>
    public IEnumerator SingleShot()
    {
        stats.Shoot(User);
        OnAttack();
        yield return new WaitForSeconds(controls.ShotDelay);
    }
    public override bool CanAttack()
    {
        //Check that the fire button is still held (or that the minimum burst hasn't yet completed)
        if ((PrimaryHeld || controls.WillBurst(shotsInBurst)) == false) return false;
        
        if (controls.CanBurst(shotsInBurst) == false) return false;

        // Don't shoot if there's not enough ammo in the magazine
        if (magazine != null && magazine.ammo.current < stats.ammoPerShot) return false;

        // If the weapon consumes ammunition, but there isn't enough to fire
        if (consumesAmmo && ammo.GetStock(stats.ammoType) < stats.ammoPerShot) return false;

        return true;
    }
    public override void OnAttack()
    {
        if (magazine != null)
        {
            magazine.ammo.current -= stats.ammoPerShot;
        }
        if (consumesAmmo)
        {
            ammo.Spend(stats.ammoType, stats.ammoPerShot);
        }
        shotsInBurst++;
    }
    void TrySendMessage(ref float timeOfLastMessage)
    {
        // Transmit telegraph message to AI, if it's the first shot or enough time has passed since the previous message transmission
        if (Time.time - timeOfLastMessage <= controls.messageDelay) return;
        
        int damage = stats.projectilePrefab.damageStats.damage;
        float spread = stats.shotSpread + User.weaponHandler.aimSwayAngle;

        DirectionalAttackMessage newMessage = new DirectionalAttackMessage(User, damage, User.LookTransform.position, User.aimDirection, stats.range, spread, stats.projectilePrefab.detection);
        Notification<AttackMessage>.Transmit(newMessage);

        timeOfLastMessage = Time.time; // Resets time
    }
}
