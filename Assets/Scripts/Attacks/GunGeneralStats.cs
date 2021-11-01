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
    public AmmunitionType ammoType;
    public int ammoPerShot = 1;
    public UnityEvent effectsOnFire;

    public void Shoot(Entity user, Vector3 origin, Vector3 aimDirection, Vector3 worldUp)
    {
        effectsOnFire.Invoke();

        for (int i = 0; i < projectileCount; i++)
        {
            Projectile newProjectile = Instantiate(projectilePrefab);
            newProjectile.spawnedBy = user;
            newProjectile.gameObject.SetActive(true);

            Vector3 spreadAngles = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            spreadAngles = spreadAngles.normalized * shotSpread;
            Vector3 individualShotDirection = MiscFunctions.AngledDirection(spreadAngles, aimDirection, worldUp);
            Vector3 hitPoint = individualShotDirection.normalized * range;
            if (Physics.Raycast(origin, individualShotDirection, out RaycastHit surfaceHit, range, projectilePrefab.detection))
            {
                hitPoint = surfaceHit.point;
            }
            
            if (Vector3.Angle(hitPoint - muzzle.position, individualShotDirection) < 90) // If hit point is in front of muzzle (like normal)
            {
                newProjectile.transform.position = muzzle.position;
                newProjectile.transform.LookAt(hitPoint, worldUp);
            }
            else // If for whatever reason the hit point is behind the muzzle
            {
                newProjectile.transform.position = hitPoint;
                newProjectile.transform.LookAt(transform.position + individualShotDirection, worldUp);
                newProjectile.OnHit(surfaceHit);
            }
        }
    }
}
