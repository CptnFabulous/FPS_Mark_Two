using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunMagazine : MonoBehaviour
{
    public int capacity = 10;
    public int current = 10;

    public float delayTime = 0.25f;

    public int roundsReloadedAtOnce = 1;
    public float delayBetweenLoads = 0.1f;
    /*
    public PlayerAction Reload()
    {



        while (current < capacity)
        {
            yield return new WaitForSeconds(delayBetweenLoads);
            current += roundsReloadedAtOnce;
        }
    }
    */
}
