using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunMagazine : MonoBehaviour
{
    public Resource ammo = new Resource(30, 30, 5);

    [Header("Sequence start")]
    public UnityEvent onReloadStart;
    public float startTransitionDelay = 0.25f;
    [Header("Sequence")]
    public UnityEvent onIncrementStart;
    public UnityEvent onIncrementEnd;
    public int roundsReloadedAtOnce = 1;
    public float delayBetweenLoads = 0.1f;
    [Header("Sequence end")]
    public UnityEvent onReloadEnd;
    public float endTransitionDelay;

    public bool currentlyReloading { get; private set; }
    IEnumerator currentSequence;

    public RangedAttack modeServing;

    AmmunitionInventory inventory => (modeServing != null) ? modeServing.ammo : null;
    AmmunitionType type => modeServing.stats.ammoType;

    private void OnEnable()
    {
        if (inventory != null)
        {
            ammo.current = Mathf.Min(ammo.current, inventory.GetStock(type));
        }
    }
    private void Update()
    {
        if (modeServing == null)
        {
            enabled = false;
            return;
        }

        //if (mode.InAction) return;

        // If there isn't enough ammo in the magazine to fire another shot
        if (ammo.current < modeServing.stats.ammoPerShot && currentlyReloading == false)
        {
            TryReload();
        }
    }
    public void TryReload()
    {
        if (CanReload == false) return;

        currentSequence = ReloadSequence();
        StartCoroutine(currentSequence);
    }


    /// <summary>
    /// Is the player able to reload their weapon (if not, magazine is full or there is no more ammo to reload with)
    /// </summary>
    public bool CanReload => ammo.current < ammo.max && ReservedAmmo(type) > 0;
    public int ReservedAmmo(AmmunitionType type)
    {
        if (inventory != null)
        {
            return (int)(inventory.GetStock(type) - ammo.current);
        }
        else
        {
            return int.MaxValue;//(int)Mathf.Infinity;
        }
        
    }


    IEnumerator ReloadSequence()
    {
        currentlyReloading = true;

        // If user is currently aiming down sights, cancel it
        GunADS ads = modeServing.optics;
        if (ads != null && ads.IsAiming)
        {
            ads.IsAiming = false;
            yield return new WaitUntil(() => !ads.IsAiming && !ads.IsTransitioning);
        }


        onReloadStart.Invoke();
        yield return new WaitForSeconds(startTransitionDelay);

        // If reload sequence has not been cancelled, magazine is not full and there is still ammo to reload with
        while (CanReload && currentlyReloading == true)
        {
            yield return IncrementReload();
        }
        // Once all rounds are reloaded, ammo is depleted or player deliberately cancels reload
        currentlyReloading = false;
        onReloadEnd.Invoke();
        yield return new WaitForSeconds(endTransitionDelay);
        EndSequence();
    }

    IEnumerator IncrementReload()
    {
        onIncrementStart.Invoke();
        //yield return new WaitForSeconds(delayBetweenLoads);

        bool cancelledDuringOperation = false;

        yield return MiscFunctions.WaitOnLerp(delayBetweenLoads, (ref float t) =>
        {
            if (CanReload) return;
            if (currentlyReloading) return;

            t = 1;
            cancelledDuringOperation = true;
        });

        if (cancelledDuringOperation)
        {
            onIncrementEnd.Invoke();
            yield break;
        }

        /*
        float t = 0;
        do
        {
            if (CanReload == false) yield break;
            if (currentlyReloading == false) yield break;

            t += Time.deltaTime / delayBetweenLoads;
            t = Mathf.Clamp01(t);
            // Update GUI to show load progress
            yield return t;
        }
        while (t < 1);
        */

        // Checks how much ammo is remaining. If less is available than what would normally be reloaded, only reload that amount
        int amountToAdd = Mathf.Min(roundsReloadedAtOnce, ReservedAmmo(type));
        ammo.Increment(amountToAdd, out _);
        onIncrementEnd.Invoke();
    }

    public void CancelReload()
    {
        currentlyReloading = false;
    }
    void EndSequence()
    {
        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
        }
        currentSequence = null;
    }
}