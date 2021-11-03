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

    public void InputLoop(RangedAttack mode, WeaponHandler user)
    {
        // If player wants to reload their weapon, and if reloading is possible
        if (WantsToReload(mode, user) && CanReload(mode.stats.ammoType, user.ammo))
        {
            currentSequence = ReloadSequence(mode, user.ammo);
            StartCoroutine(currentSequence);
        }
        else if (ReloadActive && user.primary.Pressed)
        {
            CancelReload();
        }
    }

    /// <summary>
    /// Checks if this magazine needs to be reloaded (either because it's empty or because the player deliberately pressed the reload button)
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool WantsToReload(RangedAttack mode, WeaponHandler user)
    {
        // EITHER
        // If magazine does not have enough ammo to shoot
        // If player deliberately wants to reload a half empty weapon
        // AND
        // If player is not in the middle of a reload cycle
        bool manual = user.tertiary.Pressed;
        bool automatic = ammo.current < mode.stats.ammoPerShot;
        return (manual || automatic) && ReloadActive == false;
    }
    /// <summary>
    /// Is the player able to reload their weapon (if not, magazine is full or there is no more ammo to reload with)
    /// </summary>
    public bool CanReload(AmmunitionType type, AmmunitionInventory inventory)
    {
        // If player's magazine is not empty
        // AND
        // If enough ammunition is remaining to reload weapon with
        return ammo.current < ammo.max && ReservedAmmo(type, inventory) > 0;
    }
    public int ReservedAmmo(AmmunitionType type, AmmunitionInventory inventory)
    {
        return (int)(inventory.GetStock(type) - ammo.current);
    }
    

    IEnumerator ReloadSequence(RangedAttack mode, AmmunitionInventory userAmmo)
    {
        ReloadActive = true;
        onReloadStart.Invoke();
        yield return new WaitForSeconds(startTransitionDelay);

        // If reload sequence has not been cancelled, magazine is not full and there is still ammo to reload with
        while (CanReload(mode.stats.ammoType, userAmmo) && ReloadActive == true)
        {
            yield return new WaitForSeconds(delayBetweenLoads);
            // Checks how much ammo is remaining. If less is available than what would normally be reloaded, only reload that amount
            int amountToAdd = Mathf.Min(roundsReloadedAtOnce, ReservedAmmo(mode.stats.ammoType, userAmmo));
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
        StopCoroutine(currentSequence);
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