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
    public UnityEvent onRoundsReloaded;
    public int roundsReloadedAtOnce = 1;
    public float delayBetweenLoads = 0.1f;
    [Header("Sequence end")]
    public UnityEvent onReloadEnd;
    public float endTransitionDelay;

    public bool ReloadActive { get; private set; }
    IEnumerator currentSequence;

    RangedAttack mode;
    

    public void InputLoop(RangedAttack currentMode)
    {
        mode = currentMode;
        // If player wants to reload their weapon, and if reloading is possible
        if (WantsToReload && CanReload)
        {
            currentSequence = ReloadSequence();
            StartCoroutine(currentSequence);
        }
        else if (ReloadActive && mode.User.primary.Pressed)
        {
            CancelReload();
        }
    }

    /// <summary>
    /// Checks if this magazine needs to be reloaded (either because it's empty or because the player deliberately pressed the reload button)
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool WantsToReload
    {
        get
        {
            // EITHER
            // If magazine does not have enough ammo to shoot
            // If player deliberately wants to reload a half empty weapon
            // AND
            // If player is not in the middle of a reload cycle
            bool manual = mode.User.tertiary.Pressed;
            bool automatic = ammo.current < mode.stats.ammoPerShot;
            return (manual || automatic) && ReloadActive == false;
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
            return ammo.current < ammo.max && ReservedAmmo(mode.stats.ammoType) > 0;
        }
    }
    public int ReservedAmmo(AmmunitionType type)
    {
        return (int)(mode.User.ammo.GetStock(type) - ammo.current);
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
            yield return new WaitForSeconds(delayBetweenLoads);
            // Checks how much ammo is remaining. If less is available than what would normally be reloaded, only reload that amount
            int amountToAdd = Mathf.Min(roundsReloadedAtOnce, ReservedAmmo(mode.stats.ammoType));
            ammo.Change(amountToAdd, out float leftover);
            onRoundsReloaded.Invoke();
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