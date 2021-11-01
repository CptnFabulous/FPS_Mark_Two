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


    
    public IEnumerator Reload()
    {
        onReloadStart.Invoke();
        yield return new WaitForSeconds(startTransitionDelay);

        while (ammo.current < ammo.max)
        {
            yield return new WaitForSeconds(delayBetweenLoads);
            ammo.Change(roundsReloadedAtOnce, out float leftover);
            onRoundsReloaded.Invoke();
        }

        yield return new WaitForSeconds(endTransitionDelay);

    }

}
