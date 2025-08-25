using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RangedAttack : WeaponMode
{
    public RangedAttackFiringData stats;
    public GunFireController controls;
    public GunMagazine magazine;
    public GunADS optics;

    public UnityEvent onWindup;
    public UnityEvent<bool> onStartStopFiring;


    public ADSHandler adsHandler => (User != null && User.weaponHandler != null) ? User.weaponHandler.adsHandler : null;
    public bool adsPresent => optics != null && adsHandler != null;
    public override LayerMask attackMask => stats.hitDetection;
    public int shotsInBurst { get; private set; }

    public override bool inSecondaryAction
    {
        get
        {
            // If in the middle of reloading weapon
            if (currentlyReloading) return true;

            // If player is currently aiming down sights, or still transitioning back to hipfiring
            if (adsPresent && (adsHandler.currentlyAiming || adsHandler.betweenStates)) return true;

            return false;
        }
    }
    public bool currentlyReloading => magazine != null && magazine.currentlyReloading;

    public AmmunitionInventory ammo => (User != null && User.weaponHandler != null) ? User.weaponHandler.ammo : null;
    public bool consumesAmmo => ammo != null && stats.ammoType != null && stats.ammoPerShot > 0;

    public override string hudInfo => WeaponUtility.AmmoCounterHUDDisplay(this);
    public override Resource displayedResource => ammo.GetValues(stats.ammoType);

    //Check that the fire button is still held (or that the minimum burst hasn't yet completed)
    bool shootingIntended => (PrimaryHeld || controls.WillBurst(shotsInBurst));

    private void OnEnable()
    {
        if (magazine != null)
        {
            magazine.modeServing = this;
            magazine.enabled = true;
        }

        stats.user = User;


        // TO DO: only have this run if the weapon is stored in the weapon handler
        if (adsHandler != null && User.weaponHandler.equippedWeapons.Contains(attachedTo)) adsHandler.currentAttack = this;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (magazine != null) magazine.enabled = false;

        if (adsHandler != null && adsHandler.currentAttack == this) adsHandler.currentAttack = null;
    }

    protected override void OnSecondaryInputChanged(bool held)
    {
        if (User == null) return;
        WeaponHandler handler = User.weaponHandler;
        if (handler == null) return;

        if (!adsPresent) return;
        if (adsHandler.currentAttack != this) adsHandler.currentAttack = this;
        bool desiredState = MiscFunctions.GetToggleableInput(handler.adsHandler.currentlyAiming, held, handler.toggleADS);
        handler.adsHandler.currentlyAiming = desiredState;
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

        // Resets burst
        shotsInBurst = 0;
        // Sets up the message timer to infinity, to ensure it always sends a message on the first shot.
        float timeOfLastMessage = Mathf.NegativeInfinity;

        // Windup before shooting
        float windupTime = controls.windupTime;
        if (windupTime > 0)
        {
            stats.TrySendMessage(this, ref timeOfLastMessage);
            onWindup.Invoke();

            float startOfWindup = Time.time;
            // Wait for the windup duration or until fire button is released
            yield return new WaitUntil(() =>
            {
                if (PrimaryHeld == false) return true;
                return (Time.time - startOfWindup) >= windupTime;
            });
        }

        // If player is still committing to the attack after the windup
        if (PrimaryHeld)
        {
            // Activate anything that should happen continuously while gun is firing
            // TO DO: set up enemy attack code so it also activates these continuous elements
            SetContinuousElementsActive(true);

            while (shootingIntended && CanAttack()) // Check stuff like fire button held, ammo remaining
            {
                stats.TrySendMessage(this, ref timeOfLastMessage);

                //SetContinuousElementsActive(true);

                yield return SingleShot(); // Fire shot and increment burst timer
            }

            // Turn off continuous actions
            SetContinuousElementsActive(false);

            // Wait for the cooldown, if applicable
            float cooldown = controls.burstCooldown;
            if (cooldown > 0) yield return new WaitForSeconds(cooldown);
        }

        

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
        stats.Shoot();
        OnAttack();
        yield return new WaitForSeconds(controls.ShotDelay);
    }
    public override bool CanAttack()
    {
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


    void SetContinuousElementsActive(bool active)
    {
        stats.enabled = active;
        onStartStopFiring.Invoke(active);
    }

    

    public override IEnumerator SwitchFrom()
    {
        Debug.Log("Switching away from " + this);
        // Cancel reload
        if (magazine != null)
        {
            Debug.Log("Cancelling reload");
            magazine.CancelReload();
            yield return new WaitWhile(() => magazine.inSequence);
        }
        
        if (optics != null)
        {
            Debug.Log("Cancelling ADS");
            yield return adsHandler.ChangeADSAsync(false);
        }

        yield return base.SwitchFrom();
    }
}
