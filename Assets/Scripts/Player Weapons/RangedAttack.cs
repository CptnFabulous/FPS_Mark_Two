using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : WeaponMode
{
    public GunGeneralStats stats;
    public GunFireController controls;
    public GunMagazine magazine;
    public GunADS optics;

    public bool isFiring { get; private set; }
    public int shotsInBurst { get; private set; }

    public override bool InAction
    {
        get
        {
            if (isFiring) return true;
            if (NotReloading == false) return true;

            if (optics != null)
            {
                // If player is currently aiming down sights, or still transitioning back to hipfiring
                if (optics.IsAiming == true || optics.IsTransitioning == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
    public bool NotReloading => magazine == null || magazine.ReloadActive == false;

    public bool consumesAmmo => User.ammo != null && stats.ammoType != null && stats.ammoPerShot > 0;

    public bool CanShoot()
    {
        // If gun feeds from a magazine, and there isn't enough ammo in the magazine to fire
        if (magazine != null)
        {
            // If not enough ammunition is in magazine to shoot, or the player is currently reloading
            if (magazine.ammo.current < stats.ammoPerShot || magazine.ReloadActive)
            {
                return false;
            }
        }

        // If the weapon consumes ammunition, but there isn't enough to fire
        if (consumesAmmo && User.ammo.GetStock(stats.ammoType) < stats.ammoPerShot)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Continuously fires shots until the burst timer is reached, the gun runs out of ammo, or the player lets go of the trigger.
    /// </summary>
    IEnumerator FireBurst()
    {
        isFiring = true;
        shotsInBurst = 0;
        float timeOfLastMessage = Mathf.NegativeInfinity; // Sets up the message timer, to infinity to ensure it always sends a message on the first shot.

        while (PrimaryHeld && controls.CanBurst(shotsInBurst) && CanShoot())
        {
            TrySendMessage(ref timeOfLastMessage);
            // Fire shot and increment burst timer
            yield return SingleShot();
        }

        // Wait until the fire button is released, then reset the shot timer
        yield return new WaitUntil(() => PrimaryHeld == false);
        shotsInBurst = 0;
        isFiring = false;
    }
    /// <summary>
    /// Fires a single shot and increments the burst counter.
    /// </summary>
    public IEnumerator SingleShot()
    {
        if (magazine != null)
        {
            magazine.ammo.current -= stats.ammoPerShot;
        }
        if (consumesAmmo)
        {
            User.ammo.Spend(stats.ammoType, stats.ammoPerShot);
        }

        stats.Shoot(User);

        shotsInBurst++;
        yield return new WaitForSeconds(controls.ShotDelay);
    }
    void TrySendMessage(ref float timeOfLastMessage)
    {
        // Transmit telegraph message to AI, if it's the first shot or enough time has passed since the previous message transmission
        if (Time.time - timeOfLastMessage <= controls.messageDelay) return;

        DamageEffect projectileEffect = stats.projectilePrefab.damageEffect;
        int damage = projectileEffect != null ? projectileEffect.baseDamage : int.MaxValue;
        float spread = stats.shotSpread + User.weaponHandler.standingAccuracy;

        DirectionalAttackMessage newMessage = new DirectionalAttackMessage(User, damage, User.LookTransform.position, User.aimDirection, stats.range, spread, stats.projectilePrefab.detection);
        Notification<AttackMessage>.Transmit(newMessage);

        timeOfLastMessage = Time.time; // Resets time
    }

    public override void OnSwitchTo()
    {
        if (optics != null)
        {
            optics.enabled = true;
        }
        
        magazine.Initialise(this);
    }
    public override void OnSwitchFrom()
    {
        optics.enabled = false;
        magazine.enabled = false;
    }
    protected override void OnPrimaryInputChanged(bool held)
    {
        if (held && isFiring == false && NotReloading)
        {
            StartCoroutine(FireBurst());
        }
    }
    protected override void OnSecondaryInputChanged()
    {
        if (optics == null)
        {
            return;
        }

        optics.IsAiming = SecondaryActive;
    }
    public override void OnTertiaryInput()
    {
        if (magazine != null)
        {
            magazine.OnReloadPressed();
        }
    }

    protected override void OnInterrupt()
    {
        throw new System.NotImplementedException();
    }
}
