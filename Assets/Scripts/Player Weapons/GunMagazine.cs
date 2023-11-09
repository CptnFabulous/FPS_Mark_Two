using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public bool ReloadActive { get; private set; }
    IEnumerator currentSequence;

    RangedAttack mode;

    AmmunitionInventory inventory => mode.User.ammo;
    AmmunitionType type => mode.stats.ammoType;

    public void Initialise(RangedAttack currentMode)
    {
        mode = currentMode;
        enabled = true;
    }
    private void OnEnable()
    {
        ammo.current = Mathf.Min(ammo.current, inventory.GetStock(type));
    }
    private void Update()
    {
        if (mode == null)
        {
            enabled = false;
            return;
        }

        //if (mode.InAction) return;

        // If there isn't enough ammo in the magazine to fire another shot
        if (ammo.current < mode.stats.ammoPerShot && ReloadActive == false)
        {
            TryReload();
        }
    }
    public void OnReloadPressed()
    {
        if (!ReloadActive)
        {
            TryReload();
        }
        else
        {
            CancelReload();
        }
    }
    void TryReload()
    {
        if (CanReload)
        {
            currentSequence = ReloadSequence();
            StartCoroutine(currentSequence);
        }
    }


    /// <summary>
    /// Is the player able to reload their weapon (if not, magazine is full or there is no more ammo to reload with)
    /// </summary>
    public bool CanReload
    {
        get
        {
            // If player's magazine is not empty
            // AND
            // If enough ammunition is remaining to reload weapon with
            return ammo.current < ammo.max && ReservedAmmo(type) > 0;
        }
    }
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
        ReloadActive = true;

        // If user is currently aiming down sights, cancel it
        GunADS ads = mode.optics;
        if (ads != null && ads.IsAiming)
        {
            ads.IsAiming = false;
            yield return new WaitUntil(() => !ads.IsAiming && !ads.IsTransitioning);
        }


        onReloadStart.Invoke();
        yield return new WaitForSeconds(startTransitionDelay);

        // If reload sequence has not been cancelled, magazine is not full and there is still ammo to reload with
        while (CanReload && ReloadActive == true)
        {
            onIncrementStart.Invoke();
            yield return new WaitForSeconds(delayBetweenLoads);
            // Checks how much ammo is remaining. If less is available than what would normally be reloaded, only reload that amount
            int amountToAdd = Mathf.Min(roundsReloadedAtOnce, ReservedAmmo(type));
            ammo.Increment(amountToAdd, out float leftover);
            onIncrementEnd.Invoke();
        }
        // Once all rounds are reloaded, ammo is depleted or player deliberately cancels reload
        ReloadActive = false;
        onReloadEnd.Invoke();
        yield return new WaitForSeconds(endTransitionDelay);
        EndSequence();
    }
    public void CancelReload()
    {
        ReloadActive = false;
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

/*
[System.Serializable]
public struct ButtonPromptSequence
{
    [System.Serializable]
    public struct Prompt
    {
        public KeyCode keyToPress;
        public float time;
    }

    public Prompt[] prompts;
    int index;

    IEnumerator Sequence(Prompt[] prompts)
    {
        for (index = 0; index < prompts.Length; index++)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(prompts[index].keyToPress));
            yield return new WaitForSeconds(prompts[index].time);
        }
    }
}
*/