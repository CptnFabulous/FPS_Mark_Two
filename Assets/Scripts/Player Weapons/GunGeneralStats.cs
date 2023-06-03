using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunGeneralStats : MonoBehaviour
{
    [Header("Projectile")]
    public Projectile projectilePrefab;
    public int projectileCount = 1;
    public Transform muzzle;

    [Header("Accuracy")]
    public float sway = 0.2f;
    public float shotSpread = 0;
    public float range = 300;

    [Header("Ammunition")]
    public AmmunitionType ammoType;
    public int ammoPerShot = 1;
    public bool ConsumesAmmo
    {
        get
        {
            return ammoType != null && ammoPerShot > 0;
        }
    }

    [Header("Recoil")]
    public float recoilMagnitude = 2;
    public float recoilDeviationAngle = 45;
    public AnimationCurve recoilCurve;
    public float recoilTime = 0.5f;



    public UnityEvent effectsOnFire;
    public void Shoot(Character user, Vector3 origin, Vector3 aimDirection, Vector3 worldUp)
    {
        effectsOnFire.Invoke();

        for (int i = 0; i < projectileCount; i++)
        {
            // Instantiate a projectile, enable its gameobject and specify its user
            Projectile newProjectile = Instantiate(projectilePrefab);
            newProjectile.spawnedBy = user;
            newProjectile.gameObject.SetActive(true);

            // Calculates a direction for the projectile, given random spread angles
            Vector3 spreadAngles = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * shotSpread;
            Vector3 projectileDirection = Quaternion.LookRotation(aimDirection, worldUp) * Quaternion.Euler(spreadAngles) * Vector3.forward;
            
            // Calculates a point out based on the angle and range, to aim the projectile if the subsequent raycast fails
            Vector3 hitPoint;
            List<RaycastHit> thingsHit = MiscFunctions.RaycastAllWithExceptions(origin, projectileDirection, range, projectilePrefab.detection, user.health.HitboxColliders);
            if (thingsHit.Count > 0)
            {
                thingsHit.Sort((a, b) => a.distance.CompareTo(b.distance)); // Sort entries by distance
                hitPoint = thingsHit[0].point;

                // If for some reason the hitpoint is behind the muzzle, projectile has no distance to move. Activate OnHit immediately and proceed to next projectile
                if (Vector3.Angle(hitPoint - muzzle.position, projectileDirection) > 90)
                {
                    newProjectile.transform.position = hitPoint;
                    newProjectile.transform.LookAt(transform.position + projectileDirection, worldUp);
                    newProjectile.OnHit(thingsHit[0]);
                    continue;
                }
            }
            else
            {
                hitPoint = origin + (projectileDirection * range);
            }

            // Spawn projectile at muzzle and rotate it towards the hit point
            newProjectile.transform.position = muzzle.position;
            newProjectile.transform.LookAt(hitPoint, worldUp);
        }

        if (user as Player != null && recoilMagnitude > 0)
        {
            ApplyRecoil((user as Player).movement);
        }
    }



    void ApplyRecoil(MovementController player)
    {
        float recoilAngle = Random.Range(-recoilDeviationAngle, recoilDeviationAngle);
        Vector2 recoilDirection = Quaternion.Euler(0, 0, recoilAngle) * Vector2.up * recoilMagnitude;
        //player.StartCoroutine(player.RotateAimOverTime(recoilDirection, recoilTime));
        player.StartCoroutine(player.lookControls.RotateAimOverTime(recoilDirection, recoilTime, recoilCurve));
    }
}
