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

    IEnumerator currentSequence;
    public bool CurrentlyReloading { get; private set; }

    public void StartReload()
    {
        currentSequence = ReloadSequence();
        StartCoroutine(currentSequence);
    }
    IEnumerator ReloadSequence()
    {
        CurrentlyReloading = true;
        onReloadStart.Invoke();
        yield return new WaitForSeconds(startTransitionDelay);

        while (ammo.current < ammo.max && CurrentlyReloading == true)
        {
            yield return new WaitForSeconds(delayBetweenLoads);
            ammo.Change(roundsReloadedAtOnce, out float leftover);
            onRoundsReloaded.Invoke();
        }

        CurrentlyReloading = false;
        onReloadEnd.Invoke();
        yield return new WaitForSeconds(endTransitionDelay);
        EndSequence();
    }
    public void CancelReload()
    {
        CurrentlyReloading = false;
    }
    void EndSequence()
    {
        StopCoroutine(currentSequence);
        currentSequence = null;
    }


    public bool WantsToReload(RangedAttack mode)
    {
        // EITHER
        // If magazine does not have enough ammo to shoot
        // If player deliberately wants to reload a half empty weapon
        bool manual = Input.GetButtonDown("Reload");
        bool automatic = ammo.current < mode.stats.ammoPerShot;
        return manual || automatic;
    }
    public bool CanReload(RangedAttack mode, WeaponHandler user)
    {
        // If player's magazine is not empty
        // AND
        // If enough ammunition is remaining to reload weapon with
        // AND
        // If player is not in the middle of a reload cycle
        bool magazineNotFull = ammo.current < ammo.max;
        bool moreAmmunitionAvailable = (user.ammo.GetStock(mode.stats.ammoType) - ammo.current) > 0;
        return magazineNotFull && moreAmmunitionAvailable && CurrentlyReloading == false;
    }

    [System.Serializable]
    public struct SequencePrompt
    {
        public KeyCode keyToPress;
        public float time;
    }
}
