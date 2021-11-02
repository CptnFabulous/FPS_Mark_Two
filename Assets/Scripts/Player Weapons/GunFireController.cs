using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireController : MonoBehaviour
{
    public float roundsPerMinute;
    public int maxBurst;

    public bool InBurst { get; private set; }
    

    public IEnumerator Fire(RangedAttack mode, WeaponHandler user)
    {
        InBurst = true;
        Debug.Log("Player is starting to shoot");
        int shotsInBurst = 0;
        
        while ((shotsInBurst < maxBurst || maxBurst <= 0) && mode.FireHeld && mode.CanShoot(user))
        {
            mode.SingleShot(user);
            shotsInBurst++;
            yield return new WaitForSeconds(60 / roundsPerMinute);
        }

        //Debug.Log("Player is not shooting anymore");
        InBurst = false;
    }
    
    

}