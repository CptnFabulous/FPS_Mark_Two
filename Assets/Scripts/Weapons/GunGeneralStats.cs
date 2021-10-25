using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunGeneralStats : MonoBehaviour
{
    


    public Projectile projectilePrefab;
    public int projectileCount = 1;
    public Transform muzzle;
    public float sway = 0.2f;
    public float shotSpread = 0;
    public float range = 300;
    public UnityEvent effectsOnFire;

    public void Shoot(Entity user, Vector3 origin, Vector3 forward, Vector3 up)
    {
        Vector3 swayAngles = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        swayAngles = swayAngles.normalized * sway;
        Vector3 aimDirection = MiscFunctions.AngledDirection(swayAngles, forward, up); ;

        for (int i = 0; i < projectileCount; i++)
        {
            //Vector3 angles = new Vector3(Random.Range(-shotSpread, shotSpread), Random.Range(-shotSpread, shotSpread), Random.Range(-shotSpread, shotSpread));
            
            Vector3 spreadAngles = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            spreadAngles = spreadAngles.normalized * shotSpread;
            Vector3 individualShotDirection = MiscFunctions.AngledDirection(spreadAngles, aimDirection, up);
            if (Physics.Raycast(origin, individualShotDirection, out RaycastHit surfaceHit, range, projectilePrefab.detection))
            {

            }
            else
            {

            }
        }



    }

}
