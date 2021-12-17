using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFireController : MonoBehaviour
{
    public float roundsPerMinute;
    public float ShotDelay
    {
        get
        {
            return 60 / roundsPerMinute;
        }
    }


    public int maxBurst;
    public bool CanBurst(int numberOfShots)
    {
        return numberOfShots < maxBurst || maxBurst <= 0;
    }

    public bool InBurst { get; private set; }
    

    public IEnumerator Fire(RangedAttack mode)
    {
        InBurst = true;
        int shotsInBurst = 0;
        
        while (CanBurst(shotsInBurst) && mode.User.primary.Held && mode.CanShoot())
        {
            mode.SingleShot();
            shotsInBurst++;
            yield return new WaitForSeconds(ShotDelay);
        }

        InBurst = false;
    }
    
    

}