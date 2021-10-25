using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireController : MonoBehaviour
{
    public float roundsPerMinute;
    public int maxBurst;

    IEnumerator currentlyFiring;

    public IEnumerator Fire(WeaponHandler user, GunGeneralStats stats)
    {
        
        
        int shotsInBurst = 0;

        while ((shotsInBurst < maxBurst || maxBurst <= 0) && user.AttackButtonHeld)
        {
            Transform aim = user.aimOrigin;
            stats.Shoot(user.characterUsing, aim.position, aim.forward, aim.up);

            shotsInBurst++;
            yield return new WaitForSeconds(60 / roundsPerMinute);
        }


    }

    public void Cancel()
    {
        StopCoroutine(currentlyFiring);
        currentlyFiring = null;
    }
}
