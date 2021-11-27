using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireController : MonoBehaviour
{
    public float roundsPerMinute;
    public int maxBurst;

    public bool InBurst { get; private set; }
    

    public IEnumerator Fire(RangedAttack mode)
    {
        InBurst = true;
        int shotsInBurst = 0;
        
        while ((shotsInBurst < maxBurst || maxBurst <= 0) && mode.User.primary.Held && mode.CanShoot())
        {
            mode.SingleShot();
            shotsInBurst++;
            yield return new WaitForSeconds(60 / roundsPerMinute);
        }

        InBurst = false;
    }
    
    

}