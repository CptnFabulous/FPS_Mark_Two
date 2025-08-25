using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunGeneralStats : RangedAttackFiringData
{
    public float shotSpread = 0;

    [Header("Projectile")]
    public Projectile projectilePrefab;
    public int projectileCount = 1;
    public Transform muzzle;

    public UnityEvent effectsOnFire;

    public override LayerMask hitDetection => projectilePrefab.detection;
    public override int damage => projectilePrefab.damageStats.damage;
    public override float spread => shotSpread + base.spread;

    public override void Shoot()
    {
        if (user == null) return;

        Vector3 origin = user.LookTransform.position;
        Vector3 aimDirection = user.aimDirection;
        Vector3 worldUp = user.LookTransform.up;
        
        effectsOnFire.Invoke();

        for (int i = 0; i < projectileCount; i++)
        {
            // Instantiate a projectile, enable its gameobject and specify its user
            Projectile newProjectile = Instantiate(projectilePrefab);
            newProjectile.spawnedBy = user;
            newProjectile.gameObject.SetActive(true);

            // Calculates a direction for the projectile, given random spread angles
            Vector3 spreadAngles = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * shotSpread;
            Vector3 castDirection = Quaternion.LookRotation(aimDirection, worldUp) * Quaternion.Euler(spreadAngles) * Vector3.forward;

            WeaponUtility.CalculateObjectLaunch(origin, muzzle.position, castDirection, range, newProjectile.detection, user.colliders, out _, out Vector3 hitPoint, out RaycastHit rh, out bool behindMuzzle);
            if (behindMuzzle)
            {
                // If muzzle is close enough, projectile has no distance to move. Activate OnHit immediately and proceed to next projectile
                newProjectile.transform.position = hitPoint;
                newProjectile.transform.LookAt(hitPoint, worldUp);
                newProjectile.OnHit(rh);
            }
            else // Regular launch. Spawn projectile at muzzle and rotate it towards the hit point
            {
                newProjectile.transform.position = muzzle.position;
                newProjectile.transform.LookAt(hitPoint, worldUp);
            }
        }

        ApplyRecoil();
    }



    
}
